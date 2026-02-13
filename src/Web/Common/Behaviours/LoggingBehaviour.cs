using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Web.Courses.Commands;
using Web.Users.Commands;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Web.Common.Behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingBehaviour(ILogger<TRequest> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var username = _httpContextAccessor.HttpContext?.User?.Claims?.SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            switch (request)
            {
                case AuthenticateUserCommand authRequest:
                {
                    var password = authRequest.Password;
                    authRequest.Password = string.Empty;
                    LogRequest(request, requestName, username);
                    authRequest.Password = password;
                    break;
                }
                case CreateNewUserCommand newUserRequest:
                {
                    var password = newUserRequest.Password;
                    newUserRequest.Password = string.Empty;
                    LogRequest(request, requestName, username);
                    newUserRequest.Password = password;
                    break;
                }
                case ChangePasswordCommand changePasswordCommand:
                {
                    var password = changePasswordCommand.NewPassword;
                    changePasswordCommand.NewPassword = string.Empty;
                    LogRequest(request, requestName, username);
                    changePasswordCommand.NewPassword = password;
                    break;
                }
                case CreateNewCourseCommand createCourseRequest:
                {
                    Log.Information(
                        "Discman Request: {RequestName} {Username} {CourseName} {LayoutName} {NumberOfHoles}",
                        requestName,
                        username,
                        createCourseRequest.CourseName,
                        createCourseRequest.LayoutName,
                        createCourseRequest.NumberOfHoles);
                    break;
                }
                default:
                    LogRequest(request, requestName, username);
                    break;
            }


            return Task.CompletedTask;
        }

        private static void LogRequest(TRequest request, string requestName, string username)
        {
            Log
                .ForContext("Request", request, destructureObjects: false)
                .Information("Discman Request: {RequestName} {Username}", requestName, username);
        }
    }
}
