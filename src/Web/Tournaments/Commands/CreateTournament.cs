using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Web.Infrastructure;
using Web.Tournaments.Domain;

namespace Web.Tournaments.Commands
{
    public class CreateTournamentCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class CreateTournamentCommandHandler : IRequestHandler<CreateTournamentCommand, Guid>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public CreateTournamentCommandHandler(DiscmanDbContext dbContext,  IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
        }
        
        public async Task<Guid> Handle(CreateTournamentCommand request, CancellationToken cancellationToken)
        {
            var username = _contextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var newTourney = new Tournament(request.Name, request.Start, request.End, username);
            
            _dbContext.Tournaments.Add(newTourney);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return newTourney.Id;
        }
    }
}
