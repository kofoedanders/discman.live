# src/Web — ASP.NET Core Backend

## ENTRY POINTS

- `Program.cs` — Host builder, Serilog config, NServiceBus endpoint setup
- `Startup.cs` — All DI registration: MediatR, EF Core, SignalR, JWT auth, hosted services, SPA middleware

## FEATURE FOLDER LAYOUT

Each domain (`Rounds/`, `Users/`, `Courses/`, `Tournaments/`, `Feeds/`, `Leaderboard/`) contains:
```
{Feature}/
├── {Feature}Controller.cs    # Thin API controller, dispatches via IMediator
├── Commands/                 # IRequest<T> + IRequestHandler in same file
├── Queries/                  # IRequest<T> + IRequestHandler in same file
├── Domain/                   # Entity classes (EF Core entities)
├── Handlers/                 # NServiceBus IHandleMessages event handlers
├── NSBEvents/                # NServiceBus event/message types
└── {Feature}Cache.cs         # Singleton cache (if applicable)
```

## ADD NEW ENDPOINT

1. Create `Commands/MyCommand.cs` with `IRequest<TResponse>` + `IRequestHandler<MyCommand, TResponse>`
2. Inject `DiscmanContext` (EF Core), `IHubContext<RoundsHub>` (SignalR), `IMessageSession` (NServiceBus) as needed
3. Add controller action calling `_mediator.Send(new MyCommand(...))`
4. Validator (optional): `Commands/MyCommandValidator.cs` using FluentValidation — auto-discovered

## PIPELINE BEHAVIOURS (MediatR)

Located in `Common/Behaviours/`:
- `CommandValidationBehaviour` — runs FluentValidation before handler
- `PerformanceBehaviour` — logs slow requests (>500ms)
- `UnhandledExceptionBehaviour` — catches + logs unhandled exceptions

## SIGNALR

- Hub: `Infrastructure/RoundsHub.cs` → mapped to `/roundHub`
- Auth: JWT token extracted from query string `?access_token=` (configured in `Startup.cs` OnMessageReceived)
- Push updates: inject `IHubContext<RoundsHub>` in handlers, use `HubExtensions.cs` helpers

## ADMIN AREA

- Razor Pages under `Admin/` → route prefix `/admin`
- Cookie-based auth (separate from API JWT): reads JWT from cookie, validates, issues cookie
- Policy: `"AdminOnly"` requires `ClaimTypes.Name == "kofoed"`

## BACKGROUND WORKERS

Hosted services registered in `Startup.cs`:
- `UpdateCourseRatingsWorker` — recalculates course ratings periodically
- `UpdateInActiveRoundsWorker` — cleans up stale rounds
- `ResetPasswordWorker` — processes password reset queue
- `UserEmailNotificationWorker` — sends email notifications via SendGrid

## DATABASE (EF CORE + POSTGRESQL)

- DbContext: `Infrastructure/DiscmanContext.cs`
- Entity configurations in `Infrastructure/` folder
- Database: `disclive`, schema: `disclive_production`
- Uses EF Core migrations for schema changes

## GOTCHAS

- `ApiExceptionFilter.cs` maps domain exceptions to HTTP status codes — check here if custom exceptions return wrong status
- NServiceBus concurrency limited to 1 (`LimitMessageProcessingConcurrencyTo(1)` in `Program.cs`)
- SPA dev proxy: `Startup.cs` configures proxy for `ClientApp/` (old frontend) and `new-frontend/` dev servers
- JWT secret falls back to hardcoded value in dev if `TOKEN_SECRET` missing
