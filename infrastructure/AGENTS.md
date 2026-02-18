# infrastructure/ — Deployment & Docker

## STACK

Docker Compose · nginx (reverse proxy + TLS) · Let's Encrypt (certbot) · PostgreSQL 16 · RabbitMQ

## DOCKER COMPOSE SERVICES

`docker-compose.yml` defines:
| Service | Image | Purpose |
|---------|-------|---------|
| `web` | `sp1nakr/disclive:{version}` | Main web app (ASP.NET + both React SPAs) |
| `postgres` | `postgres:16-alpine` | PostgreSQL database |
| `rabbitmq` | `rabbitmq:3.10.25-alpine` | NServiceBus transport |
| `nginx` | Custom build | Reverse proxy, TLS termination |
| `certbot` | `certbot/certbot` | Let's Encrypt certificate renewal |

ELK stack (elasticsearch, logstash, kibana) is present but **commented out**.

## CONFIGURATION FILES
```
├── docker-compose.yml
├── variables.env              # Shared env vars for services
├── nginx/
│   └── nginx.conf             # Proxy rules: / → disclive:80, WebSocket upgrade for /roundHub
├── certbot/                   # TLS cert volume mount
└── elk/                       # ELK configs (commented out in compose)
```

## DEPLOY PROCESS

Automated via `deploy.sh` (or `build.sh --deploy`):

1. `./build.sh --deploy --tag X.Y` — builds, tests, pushes Docker image, then deploys
2. Or `./deploy.sh --tag X.Y` — deploys an already-pushed image

Deploy steps: SSH to `docker` host → pull image → update `docker-compose.yml` tag via sed → `docker compose up -d web` → health check `https://next.discman.live`

## GOTCHAS

- nginx config must include WebSocket upgrade headers for SignalR (`/roundHub` path)
- No automated rollback mechanism — manual `docker-compose` only
- Certbot renewal: runs as oneshot container, nginx must reload after cert renewal
- See root AGENTS.md ANTI-PATTERNS for variables.env issues

## NGINX NOTES

- Proxies all traffic to `web:80` (container name: `discmanweb`)
- WebSocket support: `proxy_set_header Upgrade $http_upgrade` for SignalR hub
- TLS certs mounted from certbot volume
- Static assets served directly by ASP.NET (not nginx)
