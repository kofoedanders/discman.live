#!/usr/bin/env bash
set -euo pipefail

DOCKER_HOST="docker"
REMOTE_DIR="~/discman"
IMAGE_NAME="sp1nakr/disclive"
COMPOSE_SERVICE="web"
HEALTH_URL="https://next.discman.live"
HEALTH_TIMEOUT=90

BOLD='\033[1m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
RESET='\033[0m'
START_TIME=$(date +%s)
STEPS_PASSED=0
STEPS_FAILED=0
FAILED_STEPS=()

usage() {
    cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Deploy discman.live to the LAN Docker host.

Pulls the latest image, restarts the web container, and verifies health.

Options:
  --tag TAG     Image tag to deploy (default: latest pushed tag from registry)
  --no-health   Skip the health check
  -h, --help    Show this help

Examples:
  ./deploy.sh                # Deploy latest image
  ./deploy.sh --tag 2.5      # Deploy specific version
EOF
    exit 0
}

step()  { echo ""; echo -e "${CYAN}${BOLD}▸ $1${RESET}"; }
pass()  { echo -e "  ${GREEN}✔ $1${RESET}"; STEPS_PASSED=$((STEPS_PASSED + 1)); }
fail()  { echo -e "  ${RED}✘ $1${RESET}"; STEPS_FAILED=$((STEPS_FAILED + 1)); FAILED_STEPS+=("$1"); }

elapsed() { local end; end=$(date +%s); echo $(( end - START_TIME )); }

IMAGE_TAG=""
SKIP_HEALTH=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --tag)       IMAGE_TAG="$2"; shift 2 ;;
        --no-health) SKIP_HEALTH=true; shift ;;
        -h|--help)   usage ;;
        *) echo "Unknown option: $1"; usage ;;
    esac
done

echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "${BOLD}  discman.live deploy${RESET}"
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"

if [[ -n "$IMAGE_TAG" ]]; then
    FULL_IMAGE="${IMAGE_NAME}:${IMAGE_TAG}"
else
    step "Resolving current image tag from remote host"
    CURRENT_TAG=$(ssh "$DOCKER_HOST" "grep -oP 'image:.*${IMAGE_NAME}:\K[^ ]+' ${REMOTE_DIR}/docker-compose.yml" 2>/dev/null || echo "")
    if [[ -z "$CURRENT_TAG" ]]; then
        echo -e "  ${RED}Could not resolve image tag from remote compose file.${RESET}" >&2
        echo "  Use --tag to specify explicitly." >&2
        exit 1
    fi
    FULL_IMAGE="${IMAGE_NAME}:${CURRENT_TAG}"
    pass "Resolved image  →  ${FULL_IMAGE}"
fi

step "Pulling image on ${DOCKER_HOST}  →  ${FULL_IMAGE}"
if ssh "$DOCKER_HOST" "docker pull ${FULL_IMAGE}"; then
    pass "Image pulled"
else
    fail "Image pull"
    exit 1
fi

step "Restarting ${COMPOSE_SERVICE} container"
if ssh "$DOCKER_HOST" "cd ${REMOTE_DIR} && docker compose up -d ${COMPOSE_SERVICE}"; then
    pass "Container restarted"
else
    fail "Container restart"
    exit 1
fi

if [[ "$SKIP_HEALTH" == true ]]; then
    echo ""
    echo -e "  ${YELLOW}– Health check skipped${RESET}"
else
    step "Waiting for health check (${HEALTH_URL}, timeout ${HEALTH_TIMEOUT}s)"
    echo -e "  ${YELLOW}  App takes ~60s to start (NServiceBus license)${RESET}"

    WAITED=0
    HEALTHY=false
    while [[ "$WAITED" -lt "$HEALTH_TIMEOUT" ]]; do
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$HEALTH_URL" 2>/dev/null || echo "000")
        if [[ "$HTTP_CODE" == "200" ]]; then
            HEALTHY=true
            break
        fi
        sleep 5
        WAITED=$((WAITED + 5))
        echo -e "  ${YELLOW}  ${WAITED}s — HTTP ${HTTP_CODE}${RESET}"
    done

    if [[ "$HEALTHY" == true ]]; then
        pass "Health check passed (HTTP 200 after ${WAITED}s)"
    else
        fail "Health check timed out after ${HEALTH_TIMEOUT}s"
    fi
fi

echo ""
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "${BOLD}  Summary${RESET}"
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "  ${GREEN}Passed:${RESET}  ${STEPS_PASSED}"
echo -e "  ${RED}Failed:${RESET}  ${STEPS_FAILED}"
echo -e "  Image:   ${FULL_IMAGE}"
echo -e "  Host:    ${DOCKER_HOST}"
echo -e "  Time:    $(elapsed)s"

if [[ "$STEPS_FAILED" -gt 0 ]]; then
    echo ""
    echo -e "  ${RED}Failed steps:${RESET}"
    for s in "${FAILED_STEPS[@]}"; do
        echo -e "    ${RED}✘ ${s}${RESET}"
    done
    echo ""
    exit 1
fi

echo ""
exit 0
