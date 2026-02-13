using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Commands
{
    public class AddHoleCommand : IRequest<Round>
    {
        public Guid RoundId { get; set; }
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int Length { get; set; }
    }

    public class AddHoleCommandHandler : IRequestHandler<AddHoleCommand, Round>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public AddHoleCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<Round> Handle(AddHoleCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var round = await _dbContext.Rounds.SingleAsync(r => r.Id == request.RoundId, cancellationToken);

            if (!round.IsPartOfRound(username)) throw new UnauthorizedAccessException($"Cannot update round you are not part of");

            round.AddHole(request.HoleNumber, request.Par, request.Length);

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _roundsHub.NotifyPlayersOnUpdatedRound(username, round);

            return round;
        }
    }
}
