using AutoMapper;
using Web.Common.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.Courses.Queries
{
    public class CourseVm : IMapFrom<Course>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Layout { get; set; }
        public CoordinatesVm Coordinates { get; set; }

        public List<CourseHoleVm> Holes { get; set; }
        public List<string> Admins { get; set; }

        public CourseStats CourseStats { get; set; }
        public double Distance { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Course, CourseVm>()
                .ForMember(d => d.CourseStats, opt => opt.Ignore())
                .ForMember(d => d.Distance, opt => opt.Ignore())
                .ForMember(d => d.Holes, opt => opt.MapFrom(s => s.Holes.OrderBy(h => h.Number)));
        }

    }

    public class CoordinatesVm : IMapFrom<Coordinates>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}