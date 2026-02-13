using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure;

namespace Web.Courses
{
    public class UpdateCourseRatingsWorker : IHostedService, IDisposable
    {
        private readonly ILogger<UpdateCourseRatingsWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public UpdateCourseRatingsWorker(ILogger<UpdateCourseRatingsWorker> logger, IServiceScopeFactory serviceScopeFactory)
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
            var courses = dbContext.Courses.ToList();
            _logger.LogInformation($"Updating ratings of all {courses.Count} courses");
            
            foreach (var course in courses)
            {
                try
                {
                    var cutoff = DateTime.UtcNow.AddYears(-1);
                    var roundsOnCourse = dbContext.Rounds
                        .Where(r => r.CourseName == course.Name)
                        .Where(r => r.CourseLayout == course.Layout)
                        .Where(r => r.StartTime > cutoff)
                        .Where(r => r.IsCompleted)
                        .ToList();

                    if (!roundsOnCourse.Any())
                    {
                        _logger.LogInformation($"No rounds found for course {course.Id} {course.Name} {course.Layout}");
                        continue;
                    }

                    foreach (var courseHole in course.Holes)
                    {
                        var average = roundsOnCourse
                            .SelectMany(r => r.PlayerScores
                                .Select(s => s.Scores[courseHole.Number - 1]))
                            .Average(s => s.RelativeToPar);
                        courseHole.Average = average;
                    }

                    var orderedHoles = course.Holes.OrderByDescending(s => s.Average).Select(s => s.Number).ToArray();
                    foreach (var courseHole in course.Holes)
                    {
                        courseHole.Rating = Array.IndexOf(orderedHoles, courseHole.Number) + 1;
                        courseHole.Average += courseHole.Par;
                    }
                    

                    dbContext.Courses.Update(course);
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Failed to update ratings of course {course.Id} {course.Name} {course.Layout}. {e.StackTrace}");
                }
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
