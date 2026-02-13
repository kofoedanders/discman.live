using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Commands
{
    public class LeaveRoundCommand : IRequest<bool>
    {
        public Guid RoundId { get; set; }
    }

    public class LeaveRoundCommandHandler : IRequestHandler<LeaveRoundCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public LeaveRoundCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<bool> Handle(LeaveRoundCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == request.RoundId, cancellationToken);
            round.PlayerScores = round.PlayerScores.Where(s => s.PlayerName != username).ToList();

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(username, round);

            return true;
        }
    }
}
