# PROJECT KNOWLEDGE BASE

**Generated:** 2026-02-08
**Commit:** 1005a3a
**Branch:** main

## OVERVIEW

Disc golf live scoring web app ("Discman") with ASP.NET Core 9 backend, React SPA frontend, and Expo React Native mobile app. Postgres (Marten document DB) for persistence, SignalR for real-time score updates, NServiceBus+RabbitMQ for async messaging.

## STRUCTURE
```
./
├── src/Web/              # ASP.NET Core backend + React SPA (main app)
│   ├── ClientApp/        # React CRA frontend (Bulma CSS, Redux, TypeScript)
│   ├── Rounds/           # Round domain: commands, queries, handlers, domain model
│   ├── Users/            # User domain: auth, commands, queries, domain model
│   ├── Courses/          # Course domain
│   ├── Tournaments/      # Tournament domain
│   ├── Feeds/            # Activity feed domain
│   ├── Leaderboard/      # Leaderboard queries + cache
│   ├── Infrastructure/   # Marten config, SignalR hub, NServiceBus config
│   ├── Common/           # Cross-cutting: validation, exceptions, mapping, behaviours
│   └── Admin/            # Razor Pages admin area (cookie-auth, /admin route)
├── src/mobile/           # Expo React Native mobile app (SDK 39)
├── next/                 # Discman 2.0 rewrite (Blazor + event-sourced DDD)
│   ├── Domain/           # Event-sourced aggregates, value objects
│   ├── Domain.UnitTests/ # NUnit tests with Given/When/Then Scenario base class
│   └── Web/              # Blazor Server (.NET 8)
├── infrastructure/       # Docker Compose, nginx, ELK stack, certbot
└── .github/workflows/    # CI build + CodeQL scanning
```

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Add API endpoint | `src/Web/{Domain}/` | Create Command/Query + add to controller |
| Add React page | `src/Web/ClientApp/src/components/` | Add component, wire route in `App.tsx` |
| Add mobile screen | `src/mobile/screens/` | Add screen, register in `navigation/` |
| Change auth flow | `src/Web/Startup.cs` | JWT config lines 92-141 |
| Real-time updates | `src/Web/Infrastructure/RoundsHub.cs` | SignalR hub + `HubExtensions.cs` |
| Database schema | `src/Web/Infrastructure/MartenConfiguration.cs` | Marten auto-creates from C# models |
| Redux state | `src/Web/ClientApp/src/store/` | One file per domain slice |
| Background jobs | `src/Web/{Domain}/*Worker.cs` | Hosted services registered in Startup |
| Discman 2.0 domain | `next/Domain/` | Event-sourced aggregates |
| Deployment | `infrastructure/docker-compose.yml` | nginx + postgres + rabbitmq + web |
| CI pipeline | `.github/workflows/ci-build.yml` | Runs `build.sh`, pushes Docker on tag |

## ARCHITECTURE PATTERNS

- **CQRS via MediatR**: Thin controllers dispatch commands/queries. Each domain folder has `Commands/`, `Queries/`, `Handlers/`
- **Marten document DB**: PostgreSQL used as document store. Auto-creates schema. No EF Core
- **SignalR real-time**: `RoundsHub` at `/roundHub`. JWT token passed via query string for WebSocket auth
- **NServiceBus**: RabbitMQ transport for domain events (`NSBEvents/` folders). Message processing limited to 1 concurrent
- **Background workers**: `UpdateCourseRatingsWorker`, `UpdateInActiveRoundsWorker`, `ResetPasswordWorker`, `UserEmailNotificationWorker`
- **Admin area**: Razor Pages at `/admin` with cookie-based JWT auth and "AdminOnly" policy (`ClaimTypes.Name == "kofoed"`)
- **SPA hosting**: React build served from `wwwroot/`. Dev mode proxies to CRA dev server

## CONVENTIONS

- C# namespaces match folder structure (`Web.Rounds.Commands`)
- Feature-folder organization: each domain has `Commands/`, `Queries/`, `Domain/`, `Handlers/`, `NSBEvents/`
- Validators use FluentValidation, named `{Command}Validator.cs`
- Caches are singletons: `{Domain}Cache.cs`
- React components: PascalCase files, class components (React 16), connected via `react-redux`
- Redux store: one file per domain slice with action creators + reducer
- Mobile: Expo React Native, same Redux pattern, `screens/` for pages

## ANTI-PATTERNS (THIS PROJECT)

- `SendGrid` falls back to dummy key with console WARNING if env var missing
- `MartenConfiguration.cs` logs connection string to console (`Console.WriteLine(constring)`)
- `variables.env` uses mixed KEY:VALUE and KEY=VALUE syntax (docker-compose may misparse)
- Store files >1000 lines (`Rounds.ts`, `User.ts`) -- complexity hotspots
- `next/docker-compose.yml` contains hardcoded plaintext secrets
- No `.env.example` -- env vars only documented in README

## COMMANDS

```bash
# Dev (backend + SPA)
cd src/Web && dotnet watch run          # Backend with hot reload
cd src/Web/ClientApp && npm start       # React dev server (proxied)

# Dev (mobile)
cd src/mobile && expo start

# Build all
sh build.sh                            # Builds Classic + Next + ClientApp

# Docker
docker build -t discman -f src/Web/Dockerfile .
docker-compose -f infrastructure/docker-compose.yml up

# Deploy (manual)
# Tag vX.Y.Z -> CI builds and pushes to ghcr.io/spinakr/discman
# Then update infrastructure/docker-compose.yml image version
```

## ENVIRONMENT VARIABLES

| Variable | Purpose |
|----------|---------|
| `DOTNET_POSTGRES_CON_STRING` | Postgres connection string |
| `DOTNET_TOKEN_SECRET` | JWT signing key |
| `DOTNET_RABBITMQ_CON_STRING` | RabbitMQ connection |
| `SENDGRID_API_KEY` | Email sending |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment |

## NOTES

- Two solution files: `Discman.Classic.sln` (current production) and `next/Discman.Next.sln` (rewrite, early stage)
- `next/` uses event sourcing with DDD, Blazor Server, .NET 8 -- not yet deployed
- Image registry inconsistency: CI pushes to `ghcr.io/spinakr/discman`, docker-compose uses `sp1nakr/disclive`
- React app is on v16 with class components -- no hooks migration yet
- Mobile app uses Expo SDK 39 (very old, ~2020)
- No automated deployment -- manual docker-compose update after CI image push
