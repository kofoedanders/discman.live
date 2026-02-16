#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
K6_SCRIPT="$SCRIPT_DIR/perf-test.js"

BASE_URL="${BASE_URL:-}"

if [[ -z "$BASE_URL" ]]; then
    echo "Usage: BASE_URL=http://localhost:5000 $0"
    echo ""
    echo "Start the app with Testcontainers or docker-compose first,"
    echo "then set BASE_URL to the running instance."
    exit 1
fi

if ! command -v k6 &>/dev/null; then
    echo "k6 is not installed. Install: brew install k6"
    exit 1
fi

echo "Running performance tests against $BASE_URL"
echo ""

k6 run \
    --env BASE_URL="$BASE_URL" \
    --summary-export "$SCRIPT_DIR/results.json" \
    "$K6_SCRIPT"

echo ""
echo "Results exported to $SCRIPT_DIR/results.json"
