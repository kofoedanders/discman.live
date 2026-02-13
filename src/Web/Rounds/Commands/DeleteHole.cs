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
    public class DeleteHoleCommand : IRequest<bool>
    {
        public Guid RoundId { get; set; }
        public int HoleNumber { get; set; }
    }

    public class DeleteHoleCommandHandler : IRequestHandler<DeleteHoleCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public DeleteHoleCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<bool> Handle(DeleteHoleCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == request.RoundId, cancellationToken);

            if (round.CreatedBy != username) throw new UnauthorizedAccessException("Only rounds created by yourself can be modified");

            foreach (var playerScore in round.PlayerScores)
            {
                playerScore.Scores = playerScore.Scores.Where(s => s.Hole.Number != request.HoleNumber).ToList();
            }

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(username, round);

            return true;
        }
    }
}
