using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Common.Mapping;
using Web.Users;

namespace Web.Admin.Users
{
    public class Details : PageModel
    {
        private readonly IMediator _mediator;

        public Details(IMediator mediator) => _mediator = mediator;

        public Result Data { get; private set; }

        public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

        public record Query : IRequest<Result>
        {
            public string Username { get; set; }
        }

        public record Result
        {
            public UserDetailsVm User { get; init; }

            public record UserDetailsVm : IMapFrom<User>
            {
                public string Username { get; set; }
                public int DiscmanPoints { get; set; }
                public string Emoji { get; set; }
                public string Email { get; set; }
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly DiscmanDbContext _dbContext;
            private readonly IMapper _mapper;


            public Handler(DiscmanDbContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Result> Handle(Query message, CancellationToken token)
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == message.Username, token);

                return new Result
                {
                    User = _mapper.Map<Result.UserDetailsVm>(user)
                };
            }
        }
    }
}
