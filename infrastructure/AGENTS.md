# infrastructure/ — Deployment & Docker

## STACK

Docker Compose · nginx (reverse proxy + TLS) · Let's Encrypt (certbot) · PostgreSQL (with plv8) · RabbitMQ

## DOCKER COMPOSE SERVICES

`docker-compose.yml` defines:
| Service | Image | Purpose |
|---------|-------|---------|
| `disclive` | `sp1nakr/disclive:{version}` | Main web app (ASP.NET + React SPA) |
| `postgres` | `andkof/postgres-plv8` | Database (plv8 extension for Marten) |
| `rabbit` | `rabbitmq:3-management` | NServiceBus transport |
| `nginx` | `nginx:mainline-alpine` | Reverse proxy, TLS termination |
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

## DEPLOY PROCESS (MANUAL)

1. Tag `vX.Y.Z` in git → CI builds and pushes to `ghcr.io/spinakr/discman`
2. Update `docker-compose.yml` image version for `disclive` service
3. `docker --context prod compose up -d disclive`

## GOTCHAS

- nginx config must include WebSocket upgrade headers for SignalR (`/roundHub` path)
- No automated rollback mechanism — manual `docker-compose` only
- Certbot renewal: runs as oneshot container, nginx must reload after cert renewal
- See root AGENTS.md ANTI-PATTERNS for registry mismatch and variables.env issues

## NGINX NOTES

- Proxies all traffic to `disclive:80`
- WebSocket support: `proxy_set_header Upgrade $http_upgrade` for SignalR hub
- TLS certs mounted from certbot volume
- Static assets served directly by ASP.NET (not nginx)
