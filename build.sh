#!/usr/bin/env bash
set -euo pipefail

# ─── Configuration ───────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION="Discman.Classic.sln"
CLIENT_APP="src/Web/ClientApp"
DOCKERFILE="src/Web/Dockerfile"
IMAGE_NAME="sp1nakr/disclive"
DOCKER_PLATFORM="linux/amd64"

# ─── State ───────────────────────────────────────────────────────────────────
STEPS_PASSED=0
STEPS_FAILED=0
STEPS_SKIPPED=0
FAILED_STEPS=()
START_TIME=$(date +%s)
DO_DOCKER=false
DO_PUSH=false
IMAGE_TAG=""

# ─── Usage ───────────────────────────────────────────────────────────────────
usage() {
    cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Build pipeline for discman.live

Steps: restore → compile → test (backend) → test (frontend) → docker build

Options:
  --docker          Build Docker image after tests pass
  --push            Build and push Docker image (implies --docker)
  --tag TAG         Docker image tag (default: auto from git tags, e.g. 2.1)
  --skip-tests      Skip all test steps
  --skip-frontend   Skip frontend tests
  -h, --help        Show this help

Examples:
  ./build.sh                        # Build + test only
  ./build.sh --docker               # Build + test + docker image
  ./build.sh --push                 # Build + test + docker image + push
  ./build.sh --push --tag 2.5       # Same, with explicit tag
EOF
    exit 0
}

# ─── Helpers ─────────────────────────────────────────────────────────────────
BOLD='\033[1m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
RESET='\033[0m'

step() {
    echo ""
    echo -e "${CYAN}${BOLD}▸ $1${RESET}"
}

pass() {
    echo -e "  ${GREEN}✔ $1${RESET}"
    STEPS_PASSED=$((STEPS_PASSED + 1))
}

fail() {
    echo -e "  ${RED}✘ $1${RESET}"
    STEPS_FAILED=$((STEPS_FAILED + 1))
    FAILED_STEPS+=("$1")
}

skip() {
    echo -e "  ${YELLOW}– $1 (skipped)${RESET}"
    STEPS_SKIPPED=$((STEPS_SKIPPED + 1))
}

elapsed() {
    local end
    end=$(date +%s)
    echo $(( end - START_TIME ))
}

auto_tag() {
    local latest
    latest=$(git tag --sort=-v:refname | head -1 2>/dev/null || echo "")
    if [[ -z "$latest" ]]; then
        echo "2.0"
        return
    fi
    latest="${latest#v}"
    local major minor
    major="$(cut -d'.' -f1 <<< "$latest")"
    minor="$(cut -d'.' -f2 <<< "$latest")"
    echo "${major}.$((minor + 1))"
}

# ─── Parse arguments ────────────────────────────────────────────────────────
SKIP_TESTS=false
SKIP_FRONTEND=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --docker)   DO_DOCKER=true; shift ;;
        --push)     DO_DOCKER=true; DO_PUSH=true; shift ;;
        --tag)      IMAGE_TAG="$2"; shift 2 ;;
        --skip-tests)    SKIP_TESTS=true; shift ;;
        --skip-frontend) SKIP_FRONTEND=true; shift ;;
        -h|--help)  usage ;;
        *) echo "Unknown option: $1"; usage ;;
    esac
done

cd "$SCRIPT_DIR"

echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "${BOLD}  discman.live build pipeline${RESET}"
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"

# ─── Step 1: Restore ─────────────────────────────────────────────────────────
step "Restoring NuGet packages"
if dotnet restore "$SOLUTION" --verbosity quiet; then
    pass "NuGet restore"
else
    fail "NuGet restore"
    echo "Cannot continue without restore." >&2
    exit 1
fi

# ─── Step 2: Compile ─────────────────────────────────────────────────────────
step "Compiling solution"
if dotnet build "$SOLUTION" --configuration Release --no-restore --verbosity quiet; then
    pass "Compile (Release)"
else
    fail "Compile (Release)"
    echo "Cannot continue without successful build." >&2
    exit 1
fi

# ─── Step 3: Backend tests ───────────────────────────────────────────────────
step "Running backend tests"
if [[ "$SKIP_TESTS" == true ]]; then
    skip "Backend tests"
else
    if dotnet test "$SOLUTION" --configuration Release --no-build --no-restore --verbosity quiet --filter "Category!=E2E"; then
        pass "Backend tests"
    else
        fail "Backend tests"
    fi
fi

# ─── Step 4: Frontend tests ──────────────────────────────────────────────────
step "Running frontend tests"
if [[ "$SKIP_TESTS" == true || "$SKIP_FRONTEND" == true ]]; then
    skip "Frontend tests"
else
    if (cd "$CLIENT_APP" && CI=true npm test --silent 2>/dev/null); then
        pass "Frontend tests"
    else
        fail "Frontend tests"
    fi
fi

# ─── Step 5: Docker build ────────────────────────────────────────────────────
if [[ "$DO_DOCKER" == true ]]; then
    if [[ -z "$IMAGE_TAG" ]]; then
        IMAGE_TAG=$(auto_tag)
    fi
    FULL_IMAGE="${IMAGE_NAME}:${IMAGE_TAG}"

    step "Building Docker image  →  ${FULL_IMAGE}"
    if docker build --platform "$DOCKER_PLATFORM" -t "$FULL_IMAGE" -f "$DOCKERFILE" .; then
        pass "Docker build  →  ${FULL_IMAGE}"
    else
        fail "Docker build"
    fi

    # ─── Step 6: Docker push ─────────────────────────────────────────────────
    if [[ "$DO_PUSH" == true && "$STEPS_FAILED" -eq 0 ]]; then
        step "Pushing Docker image  →  ${FULL_IMAGE}"
        if docker push "$FULL_IMAGE"; then
            pass "Docker push  →  ${FULL_IMAGE}"
        else
            fail "Docker push"
        fi
    elif [[ "$DO_PUSH" == true ]]; then
        skip "Docker push (earlier failures)"
    fi
else
    skip "Docker build (use --docker or --push to enable)"
fi

# ─── Summary ─────────────────────────────────────────────────────────────────
echo ""
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "${BOLD}  Summary${RESET}"
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "  ${GREEN}Passed:${RESET}  ${STEPS_PASSED}"
echo -e "  ${RED}Failed:${RESET}  ${STEPS_FAILED}"
echo -e "  ${YELLOW}Skipped:${RESET} ${STEPS_SKIPPED}"
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

if [[ "$DO_DOCKER" == true ]]; then
    echo -e "  Image:   ${FULL_IMAGE}"
fi

echo ""
exit 0
