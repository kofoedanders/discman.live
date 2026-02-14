using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Web.Rounds;
using AutoMapper;
using Web.Infrastructure;

namespace Web.Courses.Queries
{
    public class GetCoursesQuery : IRequest<IReadOnlyList<CourseVm>>
    {
        public string Filter { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, IReadOnlyList<CourseVm>>
    {
        private readonly IMapper _mapper;
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CourseStatsCache _courseStatsCache;

        public GetCoursesQueryHandler(IMapper mapper, DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, CourseStatsCache courseStatsCache)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _courseStatsCache = courseStatsCache;
        }

        public async Task<IReadOnlyList<CourseVm>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<Course> courses;


            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {

                courses = await _dbContext.Courses
                                .Where(c => EF.Functions.ILike(c.Name, "%" + request.Filter + "%"))
                                .OrderByDescending(c => c.CreatedAt)
                                .ThenByDescending(c => c.Name)
                                .ToListAsync(cancellationToken);
            }
            else if (request.Latitude != 0 && request.Longitude != 0)
            {
                var latUpper = request.Latitude + 0.1m;
                var latLower = request.Latitude - 0.1m;
                var longUpper = request.Longitude + 0.1m;
                var longLower = request.Longitude - 0.1m;
                courses = await _dbContext.Courses
                                    .Where(c => c.Coordinates.Latitude < latUpper && c.Coordinates.Latitude > latLower)
                                    .Where(c => c.Coordinates.Longitude < longUpper && c.Coordinates.Longitude > longLower)
                                    .ToListAsync(cancellationToken);


                courses = courses
                    .OrderBy(c => CalculateDistance(c.Coordinates, new Coordinates(request.Latitude, request.Longitude)))
                    .ToList();
            }
            else
            {
                var rounds = await _dbContext.Rounds
                    .Where(r => !r.Deleted)
                    .Where(r => r.PlayerScores.Any(p => p.PlayerName == authenticatedUsername))
                    .OrderByDescending(x => x.StartTime)
                    .Take(20)
                    .ToListAsync(cancellationToken);

                var userCourses = rounds.Select(r => r.CourseName).Distinct().ToArray();
                courses = await _dbContext.Courses
                                    .Where(c => userCourses.Contains(c.Name))
                                    .OrderByDescending(c => c.CreatedAt)
                                    .ThenByDescending(c => c.Name)
                                    .ToListAsync(cancellationToken);
            }



            var courseVms = courses
                .Select(c => _mapper.Map<CourseVm>(c))
                .Select(c =>
                {
                    c.CourseStats = _courseStatsCache.GetOrCreate(c.Id, () => GetCourseStats(c.Id));
                    c.Distance = CalculateDistance(c.Coordinates, new CoordinatesVm { Latitude = request.Latitude, Longitude = request.Longitude });
                    return c;
                })
                .ToList();


            return courseVms.ToList();
        }

        public double CalculateDistance(CoordinatesVm point1, CoordinatesVm point2)
        {
            return CalculateDistance(new Coordinates(point1.Latitude, point1.Longitude), new Coordinates(point2.Latitude, point2.Longitude));
        }

        public double CalculateDistance(Coordinates point1, Coordinates point2)
        {
            var d1 = (double)point1.Latitude * (Math.PI / 180.0);
            var num1 = (double)point1.Longitude * (Math.PI / 180.0);
            var d2 = (double)point2.Latitude * (Math.PI / 180.0);
            var num2 = (double)point2.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }


        private CourseStats GetCourseStats(Guid courseId)
        {
            if (courseId == default) return new CourseStats { RoundsOnCourse = 0 };
            var roundsCount = _dbContext.Rounds
                .Count(r => r.CourseId == courseId);
            var previousRound = _dbContext.Rounds
                .Where(r => r.CompletedAt > DateTime.UtcNow.AddYears(-1) && r.CourseId == courseId)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefault();

            return new CourseStats { RoundsOnCourse = roundsCount, PreviousRound = previousRound?.CompletedAt };
        }
    }
}
