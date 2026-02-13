# Discman ETL (Marten JSONB → EF Core tables)

This ETL converts Marten JSONB documents from `mt_doc_*` tables into the EF Core relational tables created by the `InitialCreate` migration. Run it **after** restoring a production dump into your local PostgreSQL database.

## Prerequisites

- PostgreSQL 16+ (recommended; must be ≥12 for Npgsql 9+)
- EF Core schema applied (`dotnet ef database update`)
- A restored production dump containing Marten tables in schema `disclive_production` (or another schema name you supply)

## Usage

1. Restore the production dump into your local database.
2. Run the ETL scripts via the wrapper:

```bash
./infrastructure/etl/run_etl.sh \
  --db "discman" \
  --host "localhost" \
  --user "postgres" \
  --schema "disclive_production"
```

By default, the scripts assume:

- **Marten schema**: `disclive_production`
- **Target schema**: `public`

Override the schema with `--schema` when your dump uses a different casing (e.g. `disclive_Production`).

## Script Order

The run script executes ETL scripts in the following order:

1. `etl_users.sql`
2. `etl_courses.sql`
3. `etl_tournaments.sql`
4. `etl_rounds.sql`
5. `etl_feeds.sql`
6. `etl_halloffame.sql`
7. `etl_misc.sql`

## Verification

Each script prints source vs target counts. After the run completes, spot-check:

```sql
-- Users
select count(*) from disclive_production.mt_doc_user;
select count(*) from public.users;

-- Rounds
select count(*) from disclive_production.mt_doc_round;
select count(*) from public.rounds;
```

If counts differ, inspect individual inserts for missing required fields or null handling.

## Notes

- These scripts **do not** drop Marten tables or modify production.
- Re-run by truncating target tables (or restore a fresh DB).
