using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using Web.Common.Behaviours;
using Web.Courses;
using Web.Infrastructure;
using Web.Leaderboard;
using Web.Matches;
using Web.Rounds;
using Web.Tournaments;
using Web.Users;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            _env = hostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private readonly IHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (!_env.IsEnvironment("Testing"))
            {
                services.AddHostedService<UpdateCourseRatingsWorker>();
                // services.AddHostedService<DiscmanEloUpdater>();
                services.AddHostedService<UpdateInActiveRoundsWorker>();
                services.AddHostedService<ResetPasswordWorker>();
                // services.AddHostedService<DiscmanPointUpdater>();
                services.AddHostedService<UserEmailNotificationWorker>();
            }
            
            // Updated MediatR configuration for v12.x
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

            services.AddControllersWithViews(options => options.Filters.Add(new ApiExceptionFilter()));
            services.AddHttpContextAccessor();

            services.AddSendGrid(options =>
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    options.ApiKey = apiKey;
                }
                else
                {
                    // For development, provide a dummy API key
                    options.ApiKey = "SG.dummy-key-for-development";
                    Console.WriteLine("WARNING: Using dummy SendGrid API key. Set SENDGRID_API_KEY environment variable for email functionality.");
                }
            });


            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            var connectionString = Configuration.GetValue<string>("DOTNET_POSTGRES_CON_STRING");
            services.AddDbContext<DiscmanDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            
            services.AddSingleton<LeaderboardCache>();
            services.AddSingleton<UserStatsCache>();
            services.AddSingleton<TournamentCache>();
            services.AddSingleton<CourseStatsCache>();
            services.AddSingleton<PlayerCourseStatsCache>();

            var secret = Configuration.GetValue<string>("TOKEN_SECRET");
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/roundHub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            else if (context.HttpContext.Request.Path.StartsWithSegments("/Admin"))
                            {
                                context.Token = context.Request.Cookies["authentication"];
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(o =>
                o.AddPolicy("AdminOnly", p => p.RequireClaim(ClaimTypes.Name, "kofoed")));

            services.AddSignalR();
            services.AddRazorPages(o =>
            {
                o.RootDirectory = "/Admin";
                o.Conventions.AuthorizeFolder("/", "AdminOnly");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }


            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<RoundsHub>("/roundHub");
                endpoints.MapControllers();
            });


            app.Map("/admin",
                adminApp =>
                {
                    adminApp.UseRouting();
                    adminApp.UseAuthentication();
                    adminApp.UseAuthorization();
                    adminApp.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(
                            Path.Combine(env.ContentRootPath, "Admin/wwwroot")),
                    });
                    adminApp.UseEndpoints(e => { e.MapRazorPages(); });
                });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
