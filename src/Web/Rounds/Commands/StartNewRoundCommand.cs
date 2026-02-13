using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Serilog;
using Web.Courses;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;
using Web.Users;

namespace Web.Rounds.Commands
{
    public class StartNewRoundCommand : IRequest<Round>
    {
        public Guid CourseId { get; set; }
        public string RoundName { get; set; }
        public List<string> Players { get; set; }
        public ScoreMode ScoreMode { get; set; }
    }

    public class StartNewRoundCommandHandler : IRequestHandler<StartNewRoundCommand, Round>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageSession _messageSession;

        public StartNewRoundCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMessageSession messageSession)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _messageSession = messageSession;
        }

        public async Task<Round> Handle(StartNewRoundCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var playerNames = request.Players.Select(p => p.ToLower()).ToList();
            if (!playerNames.Any()) playerNames.Add(username);

            var course = _dbContext.Courses
                .Include(c => c.Holes)
                .SingleOrDefault(x => x.Id == request.CourseId);

            var players = _dbContext.Users
                .Where(u => playerNames.Contains(u.Username))
                .ToList();

            var round = course != null
                ? new Round(course, players, username, request.RoundName, request.ScoreMode)
                : new Round(players, username, request.RoundName, request.ScoreMode);

            if (course != null)
            {
                try
                {
                    CalculatePlayerCourseAverages(round, players, course);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "CalculatePlayerCourseAverages failed for course {CourseId}", course.Id);
                }
            }

            _dbContext.Rounds.Add(round);
            _dbContext.SaveChanges();

            await _messageSession.Publish(new RoundWasStarted { RoundId = round.Id });

            return round;
        }

        private void CalculatePlayerCourseAverages(Round round, List<User> players, Course course)
        {
            foreach (var player in players)
            {
                var playerCourseRounds = _dbContext.Rounds
                    .Where(r => !r.Deleted)
                    .Where(r => r.IsCompleted)
                    .Where(r => r.PlayerScores.Count > 1)
                    .Where(r => r.CourseId == course.Id)
                    .Where(r => r.PlayerScores.Any(s => s.PlayerName == player.Username))
                    .ToList();
            var fivePreviousRounds = playerCourseRounds
                .Where(r => r.StartTime < DateTime.UtcNow)
                    .OrderByDescending(r => r.StartTime)
                    .Take(5)
                    .ToList();
                var courseScores = fivePreviousRounds.Select(r => r.PlayerScore(player.Username));
                if (courseScores is null || courseScores.Count() == 0) return;
                var currentCourseAverage = courseScores.Average();

                var playerScore = round.PlayerScores.Single(s => s.PlayerName == player.Username);
                playerScore.CourseAverageAtTheTime = currentCourseAverage;
                playerScore.NumberOfHcpStrokes = (int)Math.Round(Math.Min(currentCourseAverage, course.CourseAverageScore));
            }
        }
    }
}
