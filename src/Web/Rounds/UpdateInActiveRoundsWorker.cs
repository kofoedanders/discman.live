using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Linq.SoftDeletes;
using Marten.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web.Rounds;

namespace Web.Courses
{
    public class UpdateInActiveRoundsWorker : IHostedService, IDisposable
    {
        private readonly ILogger<UpdateInActiveRoundsWorker> _logger;
        private readonly IDocumentStore _documentStore;
        private Timer _timer;

        public UpdateInActiveRoundsWorker(ILogger<UpdateInActiveRoundsWorker> logger, IDocumentStore documentStore)
        {
            _logger = logger;
            _documentStore = documentStore;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(12));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var documentSession = _documentStore.OpenSession();

            var activeRounds = documentSession
                .Query<Round>()
                .Where(r => !r.Deleted)
                .Where(r => !r.IsCompleted)
                .Where(r => r.StartTime < DateTime.Today.AddDays(-2)).ToList();

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

                documentSession.Update(round);
            }

            documentSession.SaveChanges();
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