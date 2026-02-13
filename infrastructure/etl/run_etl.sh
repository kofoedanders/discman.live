#!/usr/bin/env bash
set -euo pipefail

DB="discman"
HOST="localhost"
USER="postgres"
SCHEMA="disclive_production"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --db)
      DB="$2"; shift 2 ;;
    --host)
      HOST="$2"; shift 2 ;;
    --user)
      USER="$2"; shift 2 ;;
    --schema)
      SCHEMA="$2"; shift 2 ;;
    *)
      echo "Unknown arg: $1" >&2
      exit 1
      ;;
  esac
done

ETL_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

export ETL_SCHEMA="$SCHEMA"

psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_users.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_courses.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_tournaments.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_rounds.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_feeds.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_halloffame.sql"
psql "host=$HOST dbname=$DB user=$USER" -v ON_ERROR_STOP=1 -f "$ETL_DIR/etl_misc.sql"

echo "ETL completed successfully."
