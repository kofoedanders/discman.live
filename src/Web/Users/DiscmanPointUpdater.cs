using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Web.Infrastructure;
using Web.Rounds;
using Web.Users.Handlers;

namespace Web.Users
{
    public class DiscmanEloUpdater : IHostedService, IDisposable
    {
        private readonly ILogger<DiscmanEloUpdater> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessageSession _messageSession;
        private Timer _timer;

        public DiscmanEloUpdater(ILogger<DiscmanEloUpdater> logger, IServiceScopeFactory serviceScopeFactory,
            IMessageSession messageSession)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _messageSession = messageSession;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Thread.Sleep(5000);
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DiscmanDbContext>();

            var users = dbContext.Users.ToList();
            foreach (var user in users)
            {
                user.Elo = 1500.0;
                user.RatingHistory = new List<Rating>();
                dbContext.Users.Update(user);
            }
            dbContext.SaveChanges();

            var rounds = dbContext.Rounds.OrderBy(r => r.StartTime).ToList();
            foreach (var round in rounds)
            {
                round.RatingChanges = new List<RatingChange>();
                dbContext.Rounds.Update(round);
            }
            dbContext.SaveChanges();
            
            foreach (var round in rounds)
            {
                _messageSession.SendLocal<UpdateRatingsCommand>(e => { e.RoundId = round.Id; }).GetAwaiter()
                    .GetResult();
            }


            _logger.LogInformation("Sending UpdateRatingsCommands!!");
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
