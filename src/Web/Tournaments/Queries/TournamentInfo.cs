using System;
using System.Collections.Generic;
using AutoMapper;
using Web.Common.Mapping;
using Web.Tournaments.Domain;

namespace Web.Tournaments.Queries
{
    public class TournamentInfo : IMapFrom<Tournament>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<string> Players { get; set; }
        public List<string> Admins { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public bool HasStarted => DateTime.UtcNow.Date >= Start.Date;
        public bool IsCompleted => DateTime.UtcNow.Date > End.Date;

        public List<CourseNameAndId> Courses { get; set; }
        
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Tournament, TournamentInfo>()
                .ForMember(d => d.Courses, opt => opt.Ignore());
        }
    }

    public class CourseNameAndId
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Layout { get; set; }
    }
}