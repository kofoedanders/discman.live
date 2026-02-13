using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Courses.Commands;
using Web.Courses.Queries;

namespace Web.Courses
{
    [Authorize]
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] string filter, [FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            return Ok(await _mediator.Send(new GetCoursesQuery { Filter = filter, Latitude = latitude, Longitude = longitude }));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateNewCourseCommand request)
        {
            var newCourse = await _mediator.Send(request);
            return Ok(newCourse);
        }

        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] UpdateCourseCommand request)
        {
            var course = await _mediator.Send(new UpdateCourseCommand
            {
                CourseId = courseId,
                HoleDistances = request.HoleDistances,
                HolePars = request.HolePars,
            });

            return Ok(course);
        }
    }


}
