using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure;

namespace Web.Courses
{
    public class UpdateInActiveRoundsWorker : IHostedService, IDisposable
    {
        private readonly ILogger<UpdateInActiveRoundsWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public UpdateInActiveRoundsWorker(ILogger<UpdateInActiveRoundsWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(12));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DiscmanDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-2);
            var activeRounds = dbContext.Rounds
                .Where(r => !r.Deleted)
                .Where(r => !r.IsCompleted)
                .Where(r => r.StartTime < cutoff)
                .ToList();

            if (!activeRounds.Any()) return;

            _logger.LogInformation($"Cleaning up active rounds older than 3 days. {activeRounds.Count} active rounds will be completed or deleted");
            foreach (var round in activeRounds)
            {
                if (round.PlayerScores.SelectMany(s => s.Scores).Count(s => s.Strokes != 0) < 10)
                {
                    round.Deleted = true;
                }
                else
                {
                    round.IsCompleted = true;
                }

                dbContext.Rounds.Update(round);
            }

            dbContext.SaveChanges();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
