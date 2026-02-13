using System;
using System.Linq;
using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Web.Infrastructure;
using Web.Tournaments.Domain;
using Web.Users;

namespace Web.Tournaments.Commands
{
    public class CalculatePricesValidator : AbstractValidator<CalculatePricesCommand>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public CalculatePricesValidator(DiscmanDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;

            RuleFor(c => c.TournamentId).Must(BeAdmin).WithMessage("You must be an admin to trigger price calculations");
        }
        
        private bool BeAdmin(Guid tournamentId)
        {
            var username = _contextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var tournament = _dbContext.Tournaments.SingleOrDefault(u => u.Id == tournamentId);
            return tournament != null && tournament.Admins.Any(a => a == username);
        }
    }
}
