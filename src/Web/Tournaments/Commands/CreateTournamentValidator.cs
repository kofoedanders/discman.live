using System;
using System.Linq;
using FluentValidation;
using Marten;
using Web.Tournaments.Domain;
using Web.Users;

namespace Web.Tournaments.Commands
{
    public class CreateTournamentValidator : AbstractValidator<CreateTournamentCommand>
    {
        private readonly IDocumentSession _documentSession;

        public CreateTournamentValidator(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
            
            RuleFor(v => v.Name)
                .MinimumLength(3)
                .MaximumLength(30)
                .NotEmpty()
                .WithMessage("Name must be between 3 and 30 characters long");
            
            RuleFor(v => v.Start.Date)
                .GreaterThanOrEqualTo(DateTime.Today.Date)
                .WithMessage("Tournament must start today or later");
            
            RuleFor(v => v.End)
                .GreaterThanOrEqualTo(DateTime.Today.Date)
                .Must((cmd, end) => end.Date >= cmd.Start.Date)
                .WithMessage("End must be later than start");
           
       }
        
        private bool NotExist(string name)
        {
            return _documentSession.Query<Tournament>().SingleOrDefault(u => u.Name == name ) is null;
        }
    }
}