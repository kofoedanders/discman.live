using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Infrastructure;
using Web.Courses.Queries;

namespace Web.Courses.Commands
{
    public class CreateNewCourseCommand : IRequest<CourseVm>
    {
        public string LayoutName { get; set; }
        public string CourseName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public List<int> HolePars { get; set; }
        public List<int> HoleDistances { get; set; }
        public int NumberOfHoles { get; set; }
        public int[] Par4s { get; set; }
        public int[] Par5s { get; set; }
    }

    public class CreateNewCourseCommandHandler : IRequestHandler<CreateNewCourseCommand, CourseVm>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateNewCourseCommandHandler> _logger;

        public CreateNewCourseCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<CreateNewCourseCommandHandler> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CourseVm> Handle(CreateNewCourseCommand request, CancellationToken cancellationToken)
        {
            if (request.HoleDistances is null || !request.HoleDistances.Any())
            {
                request.HoleDistances = new int[request.NumberOfHoles].ToList();
            }

            if (request.HolePars is null || !request.HolePars.Any())
            {
                request.HolePars = new int[request.NumberOfHoles].Populate(3).ToList();
            }

            if (request.Par4s != null && request.Par4s.Length > 0)
            {
                request.HolePars = request.HolePars.Select((holePar, holeIndex) => request.Par4s.Any(p => p == holeIndex + 1) ? 4 : holePar).ToList();
            }
            if (request.Par5s != null && request.Par5s.Length > 0)
            {
                request.HolePars = request.HolePars.Select((holePar, holeIndex) => request.Par5s.Any(p => p == holeIndex + 1) ? 5 : holePar).ToList();
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.LogWarning("CreateCourse started {CourseName} {LayoutName} holes {NumberOfHoles}", request.CourseName, request.LayoutName, request.NumberOfHoles);

            var existingLayouts = await _dbContext.Courses.Where(c => c.Name == request.CourseName).ToListAsync(cancellationToken);
            _logger.LogWarning("CreateCourse loaded existing layouts in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            if (existingLayouts.Any(l => l.Layout == request.LayoutName))
            {
                throw new ArgumentException($"Layout on course {request.CourseName} with name {request.LayoutName} already exist");
            }
            var layoutsWithoutName = existingLayouts.Where(l => string.IsNullOrWhiteSpace(l.Layout));
            foreach (var layoutWithoutName in layoutsWithoutName)
            {
                layoutWithoutName.Layout = $"Main{layoutWithoutName.CreatedAt.Year}";
            }
            _dbContext.Courses.UpdateRange(layoutsWithoutName);

            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;


            var newCourse = new Course(request.CourseName, request.LayoutName, authenticatedUsername, request.HolePars, request.HoleDistances, request.Latitude, request.Longitude);

            newCourse.CreatedAt = DateTime.SpecifyKind(newCourse.CreatedAt, DateTimeKind.Utc);
            _dbContext.Courses.Add(newCourse);
            try
            {
                _logger.LogWarning("CreateCourse saving course {CourseId} at {ElapsedMs}ms", newCourse.Id, stopwatch.ElapsedMilliseconds);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("CreateCourse persisted {CourseId} at {ElapsedMs}ms", newCourse.Id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCourse failed before response {CourseName} {LayoutName}", request.CourseName, request.LayoutName);
                throw;
            }
            var courseVm = _mapper.Map<CourseVm>(newCourse);
            courseVm.Distance = 0;

            return courseVm;
        }
    }


}
