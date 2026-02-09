#!/usr/bin/env bash
set -eo pipefail
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
dotnet run --project "$SCRIPT_DIR/_build/_build.csproj" -- "$@"
