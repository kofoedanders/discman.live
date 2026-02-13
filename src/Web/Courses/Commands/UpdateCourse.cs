using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Users;

namespace Web.Courses.Commands
{
    public class UpdateCourseCommand : IRequest<Course>
    {
        public Guid CourseId { get; set; }
        public List<int> HolePars { get; set; }
        public List<int> HoleDistances { get; set; }
    }

    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Course>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateCourseCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Course> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var course = await _dbContext.Courses.SingleAsync(c => c.Id == request.CourseId, cancellationToken);
            if (course.Admins is null || course.Admins.All(a => a != authenticatedUsername)) throw new UnauthorizedAccessException();
            course.UpdateHoles(request.HolePars, request.HoleDistances);

            _dbContext.Courses.Update(course);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return course;
        }
    }
}
