# PROJECT KNOWLEDGE BASE

**Updated:** 2026-02-16
**Branch:** main

## OVERVIEW

Disc golf live scoring web app ("Discman") with ASP.NET Core 9 backend, two React SPA frontends (primary + classic), and an Expo React Native mobile app. EF Core with PostgreSQL for persistence, SignalR for real-time score updates, NServiceBus+RabbitMQ for async messaging.

## STRUCTURE
```
./
├── src/Web/              # ASP.NET Core 9 backend + React SPAs
│   ├── ClientApp/        # Classic React frontend (CRA, React 16, Redux, Bulma CSS) → served at /classic
│   ├── new-frontend/     # React frontend (Vite, React 19, Zustand, Tailwind v4) → served at /
│   ├── Rounds/           # Round domain: commands, queries, handlers, domain model
│   ├── Users/            # User domain: auth, commands, queries, domain model
│   ├── Courses/          # Course domain
│   ├── Tournaments/      # Tournament domain
│   ├── Feeds/            # Activity feed domain
│   ├── Leaderboard/      # Leaderboard queries + cache
│   ├── Infrastructure/   # EF Core DbContext, SignalR hub, NServiceBus config
│   ├── Common/           # Cross-cutting: validation, exceptions, mapping, behaviours
│   └── Admin/            # Razor Pages admin area (cookie-auth, /admin route)
├── src/mobile/           # Expo React Native mobile app (SDK 39)
├── next/                 # Discman 2.0 rewrite (Blazor + event-sourced DDD, early stage)
│   ├── Domain/           # Event-sourced aggregates, value objects
│   ├── Domain.UnitTests/ # NUnit tests with Given/When/Then Scenario base class
│   └── Web/              # Blazor Server (.NET 8)
├── infrastructure/       # Docker Compose, nginx, certbot
├── tests/                # API tests, integration tests, E2E smoke, performance
├── build.sh              # Build pipeline (restore → compile → test → docker → push → deploy)
├── deploy.sh             # Deploy to production Docker host
└── .github/workflows/    # CI build + CodeQL scanning
```

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Add API endpoint | `src/Web/{Domain}/` | Create Command/Query + add to controller |
| Add frontend page | `src/Web/new-frontend/src/pages/` | Add page component, wire route in `App.tsx` |
| Add classic frontend page | `src/Web/ClientApp/src/components/` | Add component, wire route in `App.tsx` |
| Add mobile screen | `src/mobile/screens/` | Add screen, register in `navigation/` |
| Change auth flow | `src/Web/Startup.cs` | JWT config lines 92-141 |
| Real-time updates | `src/Web/Infrastructure/RoundsHub.cs` | SignalR hub + `HubExtensions.cs` |
| Database schema | `src/Web/Infrastructure/DiscmanContext.cs` | EF Core DbContext, entity configs in `Infrastructure/` |
| Redux state | `src/Web/ClientApp/src/store/` | One file per domain slice |
| Background jobs | `src/Web/{Domain}/*Worker.cs` | Hosted services registered in Startup |
| Discman 2.0 domain | `next/Domain/` | Event-sourced aggregates |
| Deployment | `infrastructure/docker-compose.yml` | nginx + postgres + rabbitmq + web |
| CI pipeline | `.github/workflows/ci-build.yml` | Runs `build.sh`, pushes Docker on tag |

## ARCHITECTURE PATTERNS

- **CQRS via MediatR**: Thin controllers dispatch commands/queries. Each domain folder has `Commands/`, `Queries/`, `Handlers/`
- **EF Core + PostgreSQL**: Database `disclive`, schema `disclive_production`. Entity configurations in `Infrastructure/`
- **SignalR real-time**: `RoundsHub` at `/roundHub`. JWT token passed via query string for WebSocket auth
- **NServiceBus**: RabbitMQ transport for domain events (`NSBEvents/` folders). Message processing limited to 1 concurrent
- **Background workers**: `UpdateCourseRatingsWorker`, `UpdateInActiveRoundsWorker`, `ResetPasswordWorker`, `UserEmailNotificationWorker`
- **Admin area**: Razor Pages at `/admin` with cookie-based JWT auth and "AdminOnly" policy (`ClaimTypes.Name == "kofoed"`)
- **SPA hosting**: New React frontend served from `wwwroot/` at `/`, classic React build at `wwwroot/classic/` under `/classic`. Dev mode proxies to dev servers

## CONVENTIONS

- C# namespaces match folder structure (`Web.Rounds.Commands`)
- Feature-folder organization: each domain has `Commands/`, `Queries/`, `Domain/`, `Handlers/`, `NSBEvents/`
- Validators use FluentValidation, named `{Command}Validator.cs`
- Caches are singletons: `{Domain}Cache.cs`
- Classic React frontend: PascalCase files, class components (React 16), connected via `react-redux`
- Classic Redux store: one file per domain slice with action creators + reducer
- New React frontend: functional components, Zustand stores, Tailwind v4 CSS, react-router-dom v7
- Mobile: Expo React Native, same Redux pattern, `screens/` for pages

## ANTI-PATTERNS (THIS PROJECT)

- `SendGrid` falls back to dummy key with console WARNING if env var missing
- `variables.env` uses mixed KEY:VALUE and KEY=VALUE syntax (docker-compose may misparse)
- Store files >1000 lines (`Rounds.ts`, `User.ts`) -- complexity hotspots
- `next/docker-compose.yml` contains hardcoded plaintext secrets
- No `.env.example` -- env vars only documented in README

## COMMANDS

```bash
# Dev (backend + SPA)
cd src/Web && dotnet watch run          # Backend with hot reload
cd src/Web/ClientApp && npm start       # Classic React dev server (proxied)
cd src/Web/new-frontend && npm run dev  # Frontend dev server

# Dev (mobile)
cd src/mobile && expo start

# Build + test
./build.sh                              # Restore → compile → test
./build.sh --docker --tag 2.7           # + Docker image build
./build.sh --push --tag 2.7             # + push to Docker Hub
./build.sh --deploy --tag 2.7           # + deploy to production

# Deploy only (image already pushed)
./deploy.sh --tag 2.7

# Docker (local full stack)
docker compose -f infrastructure/docker-compose.yml up
```

## ENVIRONMENT VARIABLES

| Variable | Purpose |
|----------|---------|
| `DOTNET_POSTGRES_CON_STRING` | Postgres connection string |
| `TOKEN_SECRET` | JWT signing key |
| `DOTNET_RABBITMQ_CON_STRING` | RabbitMQ connection |
| `SENDGRID_API_KEY` | Email sending |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment |

## NOTES

- Two solution files: `Discman.Classic.sln` (current production) and `next/Discman.Next.sln` (rewrite, early stage)
- `next/` uses event sourcing with DDD, Blazor Server, .NET 8 -- not yet deployed
- Docker image: `sp1nakr/disclive:{tag}` on Docker Hub
- Production host: SSH alias `docker`, compose dir `~/discman/`, URL `https://discman.live`
- Old React app is on v16 with class components -- no hooks migration yet
- New React app is React 19 + Vite + Zustand + Tailwind v4, served at `/`
- Classic frontend (old React) is served at `/classic`
- Mobile app uses Expo SDK 39 (very old, ~2020)
