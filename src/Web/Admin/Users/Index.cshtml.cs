using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Common.Mapping;
using Web.Rounds;
using Web.Users;

namespace Web.Admin.Users
{
    public class Users : PageModel
    {
        private readonly IMediator _mediator;

        public Users(IMediator mediator) => _mediator = mediator;

        public Result Data { get; private set; }

        public async Task OnGetAsync() => Data = await _mediator.Send(new Query());

        public record Query : IRequest<Result>
        {
        }

        public record Result
        {
            public List<UserVm> Users { get; init; }

            public record UserVm : IMapFrom<User>
            {
                public string Username { get; set; }
                public double Elo { get; set; }
                public string Emoji { get; set; }
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
                var users = await _dbContext.Users
                    .OrderByDescending(u => u.DiscmanPoints)
                    .Take(20)
                    .ToListAsync(token);


                return new Result
                {
                    Users = users.Select(u => _mapper.Map<Result.UserVm>(u)).ToList()
                };
            }
        }
    }
}
