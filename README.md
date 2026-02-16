# Discman.live

Disc golf live scoring web application. Players track rounds in real-time with friends, view leaderboards, activity feeds, and course statistics. Built for a small community of friends.

**Production URL:** https://next.discman.live

## Architecture

| Layer | Technology | Location |
|-------|-----------|----------|
| Backend | ASP.NET Core 9, EF Core, PostgreSQL | `src/Web/` |
| Old Frontend | React 16 (CRA, Redux, Bulma CSS) | `src/Web/ClientApp/` → served at `/` |
| New Frontend | React 19 (Vite, Zustand, Tailwind v4) | `src/Web/new-frontend/` → served at `/new` |
| Real-time | SignalR WebSocket hub | `/roundHub` |
| Messaging | NServiceBus + RabbitMQ | Async domain events |
| Mobile | Expo React Native (SDK 39) | `src/mobile/` |

### Backend Patterns

- **CQRS via MediatR** — thin controllers dispatch commands/queries. Each domain folder has `Commands/`, `Queries/`, `Handlers/`
- **EF Core** with PostgreSQL as the data store (database: `disclive`, schema: `disclive_production`)
- **SignalR** hub at `/roundHub` for real-time score updates. JWT token passed via query string for WebSocket auth
- **NServiceBus** with RabbitMQ transport for domain events (`NSBEvents/` folders)
- **Background workers** for course ratings, inactive round cleanup, password reset emails, and user notifications

## Project Structure

```
./
├── src/Web/                  # ASP.NET Core 9 backend
│   ├── ClientApp/            # Old React frontend (CRA, served at /)
│   ├── new-frontend/         # New React frontend (Vite, served at /new)
│   ├── Rounds/               # Round domain
│   ├── Users/                # User/auth domain
│   ├── Courses/              # Course domain
│   ├── Tournaments/          # Tournament domain
│   ├── Feeds/                # Activity feed domain
│   ├── Leaderboard/          # Leaderboard queries + cache
│   ├── Infrastructure/       # EF Core DbContext, SignalR hub, entity configs
│   ├── Common/               # Validation, exceptions, mapping, behaviours
│   └── Admin/                # Razor Pages admin area (/admin)
├── infrastructure/           # Docker Compose, nginx, certbot, env vars
├── tests/                    # API tests, integration tests, E2E, performance
├── build.sh                  # Build pipeline script
├── deploy.sh                 # Deploy to production script
└── Discman.Classic.sln       # Solution file
```

### Domain folders follow this convention:

```
src/Web/{Domain}/
├── Commands/         # Write operations (MediatR requests)
├── Queries/          # Read operations (MediatR requests)
├── Handlers/         # Command/query handlers
├── Domain/           # Entity models, value objects
├── NSBEvents/        # NServiceBus event handlers
├── {Domain}Cache.cs  # Singleton cache (where applicable)
└── {Domain}Controller.cs
```

## Development

### Prerequisites

- .NET 9 SDK
- Node.js 22+
- Docker (for PostgreSQL + RabbitMQ, or full stack)
- SSH access to `docker` host (for deployment only)

### Running with Docker (quickest)

Start the full stack (database, rabbitmq, web app):

```bash
docker compose -f infrastructure/docker-compose.yml up
```

### Running for development

Start infrastructure services, then run the app locally with hot reload:

```bash
# 1. Start database + rabbitmq
docker compose -f infrastructure/docker-compose.yml up postgres rabbitmq -d

# 2. Start backend (hot reload)
cd src/Web && dotnet watch run

# 3. Start old frontend dev server (optional, proxied by backend)
cd src/Web/ClientApp && npm start

# 4. Start new frontend dev server (optional)
cd src/Web/new-frontend && npm run dev
```

### Environment Variables

| Variable | Purpose | Example |
|----------|---------|---------|
| `DOTNET_POSTGRES_CON_STRING` | PostgreSQL connection string | `host=postgres;database=disclive;password=...;username=postgres` |
| `TOKEN_SECRET` | JWT signing key | Any secret string |
| `DOTNET_RABBITMQ_CON_STRING` | RabbitMQ connection string | `host=rabbitmq` |
| `SENDGRID_API_KEY` | SendGrid email API key | Falls back to dummy key in dev (emails logged to console) |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` / `Production` |

## Build

The `build.sh` script runs the full pipeline: restore, compile, test, and optionally build/push/deploy Docker images.

```bash
# Build + test only
./build.sh

# Build + test + Docker image
./build.sh --docker

# Build + test + Docker image + push to Docker Hub
./build.sh --push --tag 2.7

# Full pipeline: build + test + push + deploy to production
./build.sh --deploy --tag 2.7

# Skip tests for faster iteration
./build.sh --docker --skip-tests

# Include E2E smoke tests
./build.sh --smoke

# Include k6 performance tests
./build.sh --perf
```

The Docker image is built from `src/Web/Dockerfile` as a multi-stage build that compiles the .NET backend, both frontends, and produces a runtime image tagged as `sp1nakr/disclive:{tag}`.

## Deploy

### Automated (recommended)

```bash
# Deploy as part of the build pipeline
./build.sh --deploy --tag 2.7

# Or deploy an already-pushed image
./deploy.sh --tag 2.7
```

`deploy.sh` connects to the production Docker host via SSH (host alias `docker`), pulls the image, updates the `docker-compose.yml` image tag, restarts the web container, and runs a health check against `https://next.discman.live`.

### Manual

```bash
# SSH to Docker host
ssh docker

# Pull the new image
docker pull sp1nakr/disclive:2.7

# Update docker-compose.yml with new version, then:
cd ~/discman && docker compose up -d web
```

### Production Stack

The production environment runs on a LAN Docker host behind nginx with Let's Encrypt TLS:

| Service | Image | Purpose |
|---------|-------|---------|
| `web` | `sp1nakr/disclive:{version}` | ASP.NET backend + both frontends |
| `postgres` | `clkao/postgres-plv8:11-2` | PostgreSQL database |
| `rabbitmq` | `rabbitmq:3.10.25-alpine` | NServiceBus message transport |
| `nginx` | Custom build | Reverse proxy, TLS termination |
| `certbot` | `certbot/certbot` | Let's Encrypt certificate renewal |

Configuration lives in `infrastructure/docker-compose.yml` and `infrastructure/variables.env`.

## Database

### Backup

```bash
ssh docker
docker exec -t postgres pg_dumpall -c -U postgres > dump_$(date +%d-%m-%Y"_"%H_%M_%S).sql
```

### Restore

```bash
cat your_dump.sql | docker exec -i postgres psql -U postgres
```

### Connect to production database

```bash
ssh docker
docker exec -it postgres psql -U postgres -d disclive
```
