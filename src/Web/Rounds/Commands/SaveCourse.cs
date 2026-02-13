using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web.Courses;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Commands
{
    public class SaveCourseCommand : IRequest<Course>
    {
        public Guid RoundId { get; set; }
        public string CourseName { get; set; }
    }

    public class SaveCourseCommandHandler : IRequestHandler<SaveCourseCommand, Course>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public SaveCourseCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<Course> Handle(SaveCourseCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            
            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == request.RoundId, cancellationToken);
            round.CourseName = request.CourseName;

            if (!round.IsPartOfRound(username)) throw new UnauthorizedAccessException($"Cannot update round you are not part of");

            var holes = round
                .PlayerScores
                .First().Scores
                .Select(x => new Hole(x.Hole.Number, x.Hole.Par, x.Hole.Distance))
                .ToList();

            var newCourse = new Course(request.CourseName, holes);
            
            _dbContext.Courses.Add(newCourse);
            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(username, round);

            return newCourse;
        }
    }
}
