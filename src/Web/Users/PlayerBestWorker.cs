using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure;
using Web.Rounds;
using Web.Users;

namespace Web.Courses
{
    public class PlayerBestWorker : IHostedService, IDisposable
    {
        private readonly ILogger<PlayerBestWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public PlayerBestWorker(ILogger<PlayerBestWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DiscmanDbContext>();

            var users = dbContext.Users.ToList();
            foreach (var user in users)
            {
                // var points = 0;
                var rounds = dbContext.Rounds
                    .Where(r => r.PlayerScores.Any(p => p.PlayerName == user.Username))
                    .Where(r => r.IsCompleted)
                    .Where(r => r.PlayerScores.Count > 1)
                    .ToList();



                // documentSession.Update(user);
            }

            // documentSession.SaveChanges();
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
