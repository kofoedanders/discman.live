using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Courses;
using Web.Infrastructure;
using Web.Tournaments.Domain;
using Web.Tournaments.Queries;

namespace Web.Tournaments.Commands
{
    public class AddCourseToTournamentCommand : IRequest<CourseNameAndId>
    {
        public Guid TournamentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class AddCourseToTournamentCommandHandler : IRequestHandler<AddCourseToTournamentCommand, CourseNameAndId>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public AddCourseToTournamentCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
        }

        public async Task<CourseNameAndId> Handle(AddCourseToTournamentCommand request, CancellationToken cancellationToken)
        {
            var username = _contextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var tournament = await _dbContext.Tournaments.SingleAsync(t => t.Id == request.TournamentId, cancellationToken);
            if (tournament.Admins.All(a => a != username)) throw new UnauthorizedAccessException("You must be an admin to change the tournament");
            tournament.AddCourse(request.CourseId);

            _dbContext.Tournaments.Update(tournament);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var course = await _dbContext.Courses.SingleAsync(c => c.Id == request.CourseId, cancellationToken);
            return new CourseNameAndId
            {
                Id = course.Id,
                Name = course.Name,
                Layout = course.Layout
            };
        }
    }
}
