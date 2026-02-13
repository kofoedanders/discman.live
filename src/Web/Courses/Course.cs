using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.Courses
{
    public class Course
    {

        public Course()
        {
        }

        public Course(string courseName, string layoutName, string admin, List<int> holePars, List<int> holeDistances, decimal latitude, decimal longitude)
        {
            Id = Guid.NewGuid();
            Name = courseName;
            Holes = holePars.Select((h, i) => new Hole(i + 1, h, holeDistances[i])).ToList();
            Admins = new List<string> { admin, "kofoed" };
            Layout = layoutName;
            CreatedAt = DateTime.UtcNow;
            Coordinates = new Coordinates(latitude, longitude);
        }

        public Course(string courseName, List<Hole> holes)
        {
            Id = Guid.NewGuid();
            Name = courseName;
            Holes = holes;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Layout { get; set; }
        public Coordinates Coordinates { get; set; }
        public string Country { get; set; }

        public List<Hole> Holes { get; set; }
        public List<string> Admins { get; set; }

        public void UpdateHoles(List<int> holePars, List<int> holeDistances)
        {
            Holes = Holes.Select(h => new Hole(h.Number, holePars[h.Number - 1], holeDistances[h.Number - 1], h.Rating, h.Average)).ToList();
        }

        public double CourseAverageScore => Holes.Sum(h => h.Average - h.Par);
    }

    public record Coordinates(decimal Latitude, decimal Longitude);

    public class Hole
    {
        public Hole(int number, int par, int distance, int rating = 0, double average = 0.0)
        {
            Number = number;
            Par = par;
            Distance = distance;
            Rating = rating;
            Average = average;
        }

        public int Number { get; set; }
        public int Par { get; set; }
        public int Distance { get; set; }
        public double Average { get; set; }
        public int Rating { get; set; }
    }
}
