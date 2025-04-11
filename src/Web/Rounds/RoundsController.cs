using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Rounds.Commands;
using Web.Rounds;
using Web.Rounds.Queries;

namespace Web.Rounds
{
    [Authorize]
    [ApiController]
    [Route("api/rounds")]
    public class RoundsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoundsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{roundId}")]
        public async Task<IActionResult> GetRound(Guid roundId)
        {
            var round = await _mediator.Send(new GetRoundQuery { RoundId = roundId });
            return Ok(round);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRounds([FromQuery] string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var rounds = await _mediator.Send(new GetUserRoundsQuery { Username = username, Page = page, PageSize = pageSize });
            return Ok(rounds);
        }

        [HttpPost]
        public async Task<IActionResult> StartNewRound([FromBody] NewRoundsRequest request)
        {
            var username = User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var round = await _mediator.Send(new StartNewRoundCommand
            {
                RoundName = request.RoundName,
                Players = request.Players,
                CourseId = request.CourseId,
                ScoreMode = request.ScoreMode,
            });

            if (round.CreatedBy != username) return Conflict(round);
            return Ok(round);
        }

        [HttpPost("{roundId}/holes")]
        public async Task<IActionResult> AddHole(Guid roundId, [FromBody] AddHoleRequest request)
        {
            var round = await _mediator.Send(new AddHoleCommand
            {
                HoleNumber = request.HoleNumber,
                Length = request.Length,
                Par = request.Par,
                RoundId = roundId
            });

            return Ok(round);
        }

        [HttpDelete("{roundId}/users")]
        public async Task<IActionResult> LeaveRound(Guid roundId)
        {
            await _mediator.Send(new LeaveRoundCommand { RoundId = roundId });
            return Ok();
        }

        [HttpDelete("{roundId}")]
        public async Task<IActionResult> DeleteRound(Guid roundId)
        {
            await _mediator.Send(new DeleteRoundCommand { RoundId = roundId });
            return Ok();
        }

        [HttpDelete("{roundId}/holes/{holeNumber}")]
        public async Task<IActionResult> DeleteHole(Guid roundId, int holeNumber)
        {
            await _mediator.Send(new DeleteHoleCommand { RoundId = roundId, HoleNumber = holeNumber });
            return Ok();
        }

        [HttpPut("{roundId}/scores")]
        public async Task<IActionResult> UpdateScore(Guid roundId, [FromBody] UpdatePlayerScoreCommand request)
        {
            request.RoundId = roundId;
            var round = await _mediator.Send(request);
            return Ok(round);
        }


        [HttpPut("{roundId}/complete")]
        public async Task<IActionResult> CompleteRound(Guid roundId, [FromBody] CompleteRoundCommand req)
        {
            req.RoundId = roundId;
            await _mediator.Send(req);
            return Ok();
        }

        [HttpPost("{roundId}/savecourse")]
        public async Task<IActionResult> SaveCourse(Guid roundId, [FromBody] SaveCourseRequest request)
        {
            var newCourse = await _mediator.Send(new SaveCourseCommand
            {
                RoundId = roundId,
                CourseName = request.CourseName
            });

            return Ok(newCourse);
        }

        [HttpGet("{roundId}/stats")]
        public async Task<IActionResult> GetRoundStats(Guid roundId)
        {
            return Ok(await _mediator.Send(new GetRoundStatsQuery { RoundId = roundId }));
        }

        [HttpGet("{roundId}/courseStats")]
        public async Task<IActionResult> GetCourseStatsForRound(Guid roundId)
        {
            return Ok(await _mediator.Send(new GetPlayersCourseStatsQuery { RoundId = roundId }));
        }

        [HttpGet("{roundId}/pace-data")]
        public async Task<IActionResult> GetRoundPaceData(Guid roundId)
        {
            var result = await _mediator.Send(new GetRoundPaceDataQuery { RoundId = roundId });
            
            if (result == null) return NotFound();
            
            return Ok(result);
        }
    }

    public class SaveCourseRequest
    {
        public string CourseName { get; set; }
    }

    public class ChangeScoreModeRequest
    {
        public ScoreMode ScoreMode { get; set; }
    }

    public class NewRoundsRequest
    {
        public Guid CourseId { get; set; }
        public string RoundName { get; set; }
        public List<string> Players { get; set; }
        public ScoreMode ScoreMode { get; set; }
    }

    public class AddHoleRequest
    {
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int Length { get; set; }
    }
}