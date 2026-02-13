using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web.Infrastructure;

namespace Web.Users
{
    public class ResetPasswordWorker : IHostedService, IDisposable
    {
        private readonly ILogger<ResetPasswordWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public ResetPasswordWorker(ILogger<ResetPasswordWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DiscmanDbContext>();

            var expiredRequests = dbContext.ResetPasswordRequests
                .Where(r => r.CreatedAt.AddHours(2) < DateTime.UtcNow)
                .ToList();

            foreach (var expiredRequest in expiredRequests)
            {
                dbContext.ResetPasswordRequests.Remove(expiredRequest);
                Log.Information($"Deleting expired reset password request for email {expiredRequest.Email} {expiredRequest.Id}");
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
