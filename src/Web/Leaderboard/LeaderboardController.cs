using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Leaderboard.Queries;

namespace Web.Leaderboard
{
    [Authorize]
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILogger<LeaderboardController> _logger;
        private readonly IMediator _mediator;

        public LeaderboardController(ILogger<LeaderboardController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard([FromQuery] bool onlyFriends, [FromQuery] int month = 0)
        {
            var playersStats = await _mediator.Send(new GetLeaderboardQuery {OnlyFriends = onlyFriends, Month = month});
            return Ok(playersStats);
        }

        [HttpGet("hallOfFame")]
        public async Task<IActionResult> GetHallOfFame()
        {
            return Ok(await _mediator.Send(new GetHallOfFameQuery()));
        }
    }
}
