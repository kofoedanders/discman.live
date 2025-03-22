using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Web.Common.Behaviours
{
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnhandledExceptionBehaviour(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                var authedUser = _httpContextAccessor.HttpContext?.User?.Claims?.SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if (authedUser != null) LogContext.PushProperty("Username", authedUser);
                Log
                    .ForContext("Username", authedUser)
                    .ForContext("Exception", ex, destructureObjects: true)
                    .Error(ex, "Unhandled exception when handling request {RequestName}", requestName);

                throw;
            }
        }
    }
}