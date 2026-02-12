# Discman: EF Core Migration

## TL;DR

> **Quick Summary**: Migrate the Discman disc golf app from Marten document DB to EF Core (relational), upgrade all dependencies to latest (.NET 10, MediatR 14, NServiceBus 10, etc.), and create SQL ETL scripts for data migration. The app continues running locally the same way as today (`dotnet watch run` + Docker containers for postgres and rabbitmq). DNS, TLS, nginx, and infrastructure hosting are out of scope — the user handles those separately.
> 
> **Deliverables**:
> - All 8 Marten document types replaced with EF Core entities and relational tables
> - All ~69 IDocumentSession injection points replaced with DbContext
> - .NET 10 + all major package upgrades applied
> - SQL ETL scripts for migrating production data from Marten JSONB to relational tables (user-executed)
> 
> **Estimated Effort**: XL
> **Parallel Execution**: YES — some waves can overlap
> **Critical Path**: .NET 10 upgrade → EF Core infra → Domain migrations → Remove Marten → ETL scripts

---

## Context

### Original Request
Migrate the app to EF Core. Consolidate existing GitHub issues #10–#21 into one revised plan. The app currently runs in production on Digital Ocean as containers (web, postgres, rabbitmq). The user wants to eventually move hosting to a local Docker host, but infrastructure/DNS/TLS changes are out of scope for this plan — the user will handle those separately. The immediate goal is getting the app fully migrated to EF Core so it works locally with `dotnet watch run` + Docker containers for postgres and rabbitmq (same dev setup as today).

### Interview Summary
**Key Discussions**:
- **EF Core only** (no Dapper): Simpler approach, one ORM for everything
- **Keep RabbitMQ + NServiceBus**: Maintain existing async messaging architecture
- **Include dependency upgrades**: .NET 10, MediatR 14, NServiceBus 10, AutoMapper 16, etc.
- **Manual verification**: No automated test suite
- **ETL scripts**: Agent creates the scripts, user runs them manually from production to local
- **Infrastructure out of scope**: DNS, TLS, nginx, docker-compose for hosting — user handles separately
- **Docker image build**: Manual process (`docker build -t sp1nakr/disclive:<version> . ; docker push`), GitHub Actions CI is NOT used

**Research Findings**:
- 8 Marten document types, ~69 files inject IDocumentSession, 7 workers inject IDocumentStore
- Round is most complex (4-level nesting: Round→PlayerScore→HoleScore→StrokeSpec)
- No event sourcing in classic app — simplifies migration significantly
- PostgreSQL 11 (clkao/postgres-plv8:11-2) must be upgraded — Npgsql 10 requires PG 12+
- Env var naming inconsistency: DOTNET_TOKEN_SECRET vs TOKEN_SECRET
- Docker image built and pushed manually: `docker build -t sp1nakr/disclive:<version> . ; docker push sp1nakr/disclive:<version>`, then version updated in docker-compose and `docker compose up -d web`. GitHub Actions CI exists but is NOT used.

### Metis Review
**Identified Gaps** (addressed):
- PostgreSQL 11 must be upgraded to 16+ before EF Core migration (Npgsql 10 requirement) — user handles postgres upgrade as part of their infra work; ETL scripts will document the PG 16+ requirement
- NServiceBus persistence must use `NServiceBus.Persistence.Sql` (no EF Core persistence package exists)
- `Achievements` class uses reflection + ICollection with private backing field — needs explicit EF Core config
- `TournamentPrices` deeply nested (7+ sub-objects) — map as JSONB column, not relational tables
- `Round.DurationMinutes` is computed property — must be `[NotMapped]`
- `Coordinates` record type needs owned entity mapping
- `HallOfFame` has inheritance hierarchy — use TPH with discriminator
- `List<string>` properties (Friends, Admins, etc.) — use PostgreSQL `text[]` arrays via Npgsql
- `User` optimistic concurrency — use `UseXminAsConcurrencyToken()` in EF Core
- `StorageExtensions.cs` extension methods on IDocumentSession need conversion

---

## Work Objectives

### Core Objective
Replace the entire Marten document DB layer with EF Core relational persistence, upgrade the .NET stack to current versions, and create SQL ETL scripts for data migration — all while keeping the current production running on Digital Ocean untouched.

### Concrete Deliverables
- `DiscmanDbContext` with all entity configurations
- EF Core migrations creating ~20 relational tables from 8 document types
- All IDocumentSession/IDocumentStore references replaced with DbContext
- All NuGet packages upgraded to latest majors
- SQL ETL scripts for migrating Marten JSONB to relational tables (user-executed)

### Definition of Done
- [ ] App starts locally with `dotnet watch run` and connects to Docker postgres + rabbitmq
- [ ] All existing features work (manual verification by user)
- [ ] Zero references to Marten in codebase
- [ ] `dotnet build` succeeds
- [ ] SQL ETL scripts exist and are documented

### Must Have
- EF Core entity configurations for all 8 document types
- Relational tables with proper foreign keys and indexes
- SQL ETL scripts for data migration
- NServiceBus 10 with SQL persistence
- .NET 10 + all major dependency upgrades

### Must NOT Have (Guardrails)
- Do NOT optimize the leaderboard "load all rounds into memory" pattern during migration
- Do NOT fix broken achievement evaluations (they return false with "logic does not work" comments — migrate broken code as-is)
- Do NOT resurrect commented-out workers (DiscmanEloUpdater, DiscmanPointUpdater) or ELK stack
- Do NOT touch React ClientApp, mobile app, or `next/` folder
- Do NOT write automated tests (manual verification only)
- Do NOT refactor namespace organization
- Do NOT create separate join tables for TournamentPrices (use JSONB column)
- Do NOT add features or change behavior — this is a persistence/infrastructure migration only
- Do NOT modify docker-compose files, nginx config, or infrastructure hosting setup
- Do NOT set up TLS/DNS/certbot
- Do NOT modify GitHub Actions CI (it is not used)

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION IN VERIFICATION**
>
> ALL verification/QA steps in this plan MUST be executable by the agent WITHOUT human action.
> The executing agent will directly verify each deliverable by running commands.
>
> **EXCEPTION: Environment prerequisites**
> The dev environment must have Docker running with postgres and rabbitmq containers available
> (same as the current development setup). The agent will verify containers are reachable
> but will not provision them.

### Test Decision
- **Infrastructure exists**: NO (no test framework set up)
- **Automated tests**: None (user decision)
- **Framework**: N/A

### Agent-Executed QA Scenarios (MANDATORY — ALL tasks)

Every task includes verification scenarios using:

| Type | Tool | How Agent Verifies |
|------|------|-------------------|
| **Database** | Bash (psql/docker exec) | Row counts, schema inspection, data queries |
| **API/Backend** | Bash (curl) | HTTP requests, response codes, JSON parsing |
| **Build** | Bash (dotnet build/publish) | Build output, exit codes |

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
└── Task 1: Upgrade .NET to 10 + upgrade all NuGet dependencies

Wave 2 (After Wave 1):
├── Task 2: Create EF Core DbContext + entity configurations + migrations
└── Task 3: Update NServiceBus 8→10 + SQL persistence

Wave 3 (After Wave 2 — Domain Migrations, partially parallel):
├── Task 4: Migrate Feeds + Leaderboard domains
├── Task 5: Migrate Courses + Tournaments domains
└── Task 6: Migrate Users domain

Wave 4 (After Wave 3):
└── Task 7: Migrate Rounds domain (depends on Users)

Wave 5 (After Wave 4):
└── Task 8: Migrate Admin area + remove Marten

Wave 6 (After Wave 5):
└── Task 9: Create SQL ETL scripts (user-executed)
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2, 3 | None |
| 2 | 1 | 4, 5, 6 | 3 |
| 3 | 1 | 8 | 2 |
| 4 | 2 | 8 | 5, 6 |
| 5 | 2 | 8 | 4, 6 |
| 6 | 2 | 7, 8 | 4, 5 |
| 7 | 6 | 8 | None |
| 8 | 4, 5, 6, 7, 3 | 9 | None |
| 9 | 8 | None | None |

### Agent Dispatch Summary

| Wave | Tasks | Recommended Agents |
|------|-------|-------------------|
| 1 | 1 | task(category="unspecified-high", load_skills=[], run_in_background=false) |
| 2 | 2, 3 | dispatch parallel after Wave 1 completes |
| 3 | 4, 5, 6 | dispatch parallel after Wave 2 completes |
| 4 | 7 | sequential after Wave 3 completes |
| 5 | 8 | sequential after Wave 4 completes |
| 6 | 9 | sequential after Wave 5 completes |

---

## Step-by-step Checkpoints (safe stopping points)

Each checkpoint is designed so you can pause (days/weeks) without losing progress.

### Checkpoint A — After Task 1 (stack upgrade)

**Stop when**:
- `dotnet build src/Web/Web.csproj` is green on .NET 10

**Verify (agent-executable)**:
```bash
dotnet --info
dotnet build src/Web/Web.csproj
```

### Checkpoint B — After Tasks 2–3 (EF Core + NServiceBus platform)

**Stop when**:
- EF Core migration applies to a clean DB
- App starts locally and connects to postgres + rabbitmq

**Verify (agent-executable)**:
```bash
dotnet ef database update --project src/Web/Web.csproj
dotnet run --project src/Web/Web.csproj
```

### Checkpoint C — After Tasks 4–6 (most domains migrated)

**Stop when**:
- Feeds/Leaderboard, Courses/Tournaments, Users have **no Marten usage**

**Verify (agent-executable)**:
```bash
grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Feeds/ src/Web/Leaderboard/ src/Web/Courses/ src/Web/Tournaments/ src/Web/Users/
dotnet build src/Web/Web.csproj
```

### Checkpoint D — After Task 7 (Rounds migrated)

**Stop when**:
- Rounds + `RoundsHub` have **no Marten usage**

**Verify (agent-executable)**:
```bash
grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Rounds/
grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Infrastructure/RoundsHub.cs
dotnet build src/Web/Web.csproj
```

### Checkpoint E — After Task 8 (Marten removed)

**Stop when**:
- Marten packages/config are gone
- Repo-wide grep finds **zero** Marten references

**Verify (agent-executable)**:
```bash
grep -r "Marten\|IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/ --include="*.cs"
dotnet build src/Web/Web.csproj
```

### Checkpoint F — After Task 9 (ETL scripts ready)

**Stop when**:
- ETL scripts + run script + docs exist

**Verify (agent-executable)**:
```bash
ls infrastructure/etl/*.sql
test -x infrastructure/etl/run_etl.sh
```

## TODOs

- [x] 1. Upgrade .NET to 10 and all NuGet dependencies

  **What to do**:
  - Update `src/Web/Web.csproj` target framework from `net9.0` to `net10.0`
  - Update Dockerfile base images from `mcr.microsoft.com/dotnet/sdk:9.0` and `aspnet:9.0` to `10.0`
  - Upgrade Microsoft packages:
    - `Microsoft.AspNetCore.Authentication.JwtBearer` → 10.x
    - `Microsoft.AspNetCore.SpaServices.Extensions` → 10.x (check if still available/needed in .NET 10)
  - Upgrade MediatR 12.2.0 → 14.x:
    - Fix registration changes in Startup.cs (MediatR 14 changes `AddMediatR` API)
    - Verify pipeline behaviours in `Common/Behaviours/` still compile
  - Upgrade NServiceBus 8.1.6 → 10.x:
    - Update `NServiceBus.Extensions.Hosting` 2.0.0 → 4.x
    - Update `NServiceBus.RabbitMQ` 8.0.3 → 11.x
    - Fix `NServiceBusConfiguration.cs` and Program.cs endpoint setup
    - Add `NServiceBus.Persistence.Sql` for saga/outbox persistence (needed for NServiceBus 10)
    - Verify all `IHandleMessages` handlers in `NSBEvents/` folders
  - Upgrade remaining packages:
    - `AutoMapper` 13.0.1 → 16.x (may need mapping profile changes)
    - `FluentValidation` 11.9.0 → 12.x
    - `Serilog.AspNetCore` 8.0.1 → 10.x
    - `Swashbuckle.AspNetCore` 8.0.0 → 10.x
    - `SendGrid` minor version bump
  - Verify `Baseline` package (v4.1.0 used in Round.cs) is compatible with .NET 10
  - Update `build.sh` if needed
  - Note: GitHub Actions CI (`ci-build.yml`) is NOT used — images are built and pushed manually. No CI changes needed.
  - **Standardize env var naming**: Update `Startup.cs` to read `DOTNET_TOKEN_SECRET` instead of `TOKEN_SECRET` (aligning with the `DOTNET_` prefix used by other env vars).

  **Must NOT do**:
  - Do NOT change database layer yet (Marten stays until EF Core is ready)
  - Do NOT refactor any business logic
  - Do NOT upgrade React or Node.js in ClientApp

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
    - Reason: Multi-package upgrade with breaking changes across MediatR, NServiceBus, AutoMapper
  - **Skills**: []
    - No specialized skills — this is package upgrade + compilation fixes

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Wave 1 (sole task)
  - **Blocks**: Tasks 2, 3
  - **Blocked By**: None

  **References**:

  **Pattern References**:
  - `src/Web/Web.csproj` — All current package versions and target framework
  - `src/Web/Startup.cs` — MediatR registration, hosted service registration, auth config
  - `src/Web/Program.cs` — NServiceBus endpoint setup, Serilog config
  - `src/Web/Infrastructure/NServiceBusConfiguration.cs` — NServiceBus transport and endpoint config
  - `src/Web/Common/Behaviours/` — MediatR pipeline behaviours (may have breaking API changes)
  - `src/Web/Dockerfile` — Base image versions to update

  **API/Type References**:
  - All `NSBEvents/` handler files in each domain folder — IHandleMessages implementations
  - `src/Web/Rounds/Domain/Round.cs` — Uses `Baseline` package (check compatibility)

  **External References**:
  - MediatR 14 migration guide: https://github.com/jbogard/MediatR/releases
  - NServiceBus 10 upgrade guide: https://docs.particular.net/nservicebus/upgrades/
  - AutoMapper 16 changes: https://github.com/AutoMapper/AutoMapper/releases

  **WHY Each Reference Matters**:
  - Web.csproj: Source of truth for all package versions — start here
  - NServiceBusConfiguration.cs: NServiceBus 10 has breaking changes in endpoint/transport config
  - Common/Behaviours: MediatR pipeline API changes between 12 and 14
  - Round.cs/Baseline: Unknown .NET 10 compatibility — could block the upgrade

  **Acceptance Criteria**:

  - [ ] `dotnet build src/Web/Web.csproj` succeeds with zero errors
  - [ ] All packages on latest major versions
  - [ ] Dockerfile uses .NET 10 SDK and runtime images

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Project builds successfully on .NET 10
    Tool: Bash
    Preconditions: .NET 10 SDK installed
    Steps:
      1. dotnet restore src/Web/Web.csproj
      2. Assert: Exit code 0, no package resolution errors
      3. dotnet build src/Web/Web.csproj --no-restore
      4. Assert: Exit code 0, "Build succeeded" in output
      5. dotnet publish src/Web/Web.csproj --no-build -o /tmp/discman-publish
      6. Assert: Exit code 0, Web.dll exists in output
    Expected Result: Full build pipeline succeeds
    Evidence: Build output captured

  Scenario: Docker image builds with .NET 10
    Tool: Bash
    Preconditions: Docker installed, Dockerfile updated
    Steps:
      1. docker build -t discman:net10-test -f src/Web/Dockerfile .
      2. Assert: Exit code 0, no build errors
      3. docker run --rm discman:net10-test dotnet --info
      4. Assert: Output shows .NET 10
    Expected Result: Container builds and runs on .NET 10
    Evidence: Docker build output captured
  ```

  **Commit**: YES
  - Message: `chore: upgrade to .NET 10 and update all major dependencies`
  - Files: `src/Web/Web.csproj`, `src/Web/Dockerfile`, `src/Web/Startup.cs`, `src/Web/Program.cs`, `src/Web/Infrastructure/NServiceBusConfiguration.cs`, `src/Web/Common/Behaviours/*.cs`, `build.sh`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 2. Create EF Core DbContext, entity configurations, and initial migration

  **What to do**:
  - Add NuGet packages: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`
  - Create `src/Web/Infrastructure/DiscmanDbContext.cs` with:
    - DbSets for all entity types: Users, Rounds, Courses, Tournaments, GlobalFeedItems, UserFeedItems, HallOfFame, ResetPasswordRequests, PlayerCourseStats
    - Entity configurations in `OnModelCreating` or separate `IEntityTypeConfiguration<T>` classes
  - Entity mapping decisions (from Metis review):
    - **User**: `UseXminAsConcurrencyToken()` for optimistic concurrency; `Friends` as `text[]` array; `Achievements` as separate table with TPH discriminator; `RatingHistory` as separate `user_ratings` table
    - **Round**: Normalize 4-level nesting to separate tables (rounds, player_scores, hole_scores, stroke_specs); `[NotMapped]` on `DurationMinutes` (computed property); `HasQueryFilter(r => !r.Deleted)` for soft-delete
    - **Course**: `Holes` as separate `course_holes` table; `Admins` as `text[]`; `Coordinates` as `[Owned]` entity
    - **Tournament**: `Players`/`Admins`/`CourseGuids` as `text[]` or UUID arrays; `TournamentPrices` as JSONB column (not relational — too deeply nested)
    - **Feeds**: `GlobalFeedItem` and `UserFeedItem` as flat tables with indexes; `Likes`/`Subjects` as `text[]` arrays
    - **HallOfFame**: TPH inheritance with discriminator; singleton pattern (single row with well-known ID)
    - **PlayerCourseStats**: Flat table, straightforward mapping
    - **ResetPasswordRequest**: Flat table, straightforward mapping
  - Register DbContext in DI alongside Marten (dual-registration period):
    ```csharp
    services.AddDbContext<DiscmanDbContext>(options =>
        options.UseNpgsql(configuration.GetValue<string>("DOTNET_POSTGRES_CON_STRING")));
    ```
  - Create initial EF Core migration: `dotnet ef migrations add InitialCreate`
  - Set up `IDbContextFactory<DiscmanDbContext>` for background workers
  - Verify migration can be applied to a clean postgres:16 database

  **Must NOT do**:
  - Do NOT remove Marten registration yet — both must coexist
  - Do NOT change any command/query handlers to use DbContext yet
  - Do NOT create data migration scripts yet (that's Task 9)

  **Recommended Agent Profile**:
  - **Category**: `ultrabrain`
    - Reason: Complex entity mapping decisions — 4-level nesting, inheritance hierarchies, JSONB columns, owned entities, concurrency tokens
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 3)
  - **Blocks**: Tasks 4, 5, 6
  - **Blocked By**: Task 1

  **References**:

  **Pattern References**:
  - `src/Web/Infrastructure/MartenConfiguration.cs` — Current DB config (DI registration pattern to mirror)
  - `src/Web/Startup.cs` — Where to add DbContext registration

  **API/Type References** (ALL domain models to map):
  - `src/Web/Rounds/Domain/Round.cs` — Round document (PlayerScore, HoleScore, StrokeSpec, PlayerSignature, RatingChange nested types)
  - `src/Web/Users/Domain/User.cs` — User document (`[UseOptimisticConcurrency]`, byte[] Password/Salt, Achievements ICollection)
  - `src/Web/Users/Domain/Achievements.cs` — Achievement base class + subclasses (reflection-based discovery via `Evaluate()`)
  - `src/Web/Users/Domain/UserStats.cs` — User stats value object
  - `src/Web/Users/Domain/ResetPasswordRequest.cs` — Reset password document
  - `src/Web/Users/Domain/PlayerBest.cs` — Player best scores
  - `src/Web/Courses/Course.cs` — Course document (Holes list, Coordinates record, Admins list)
  - `src/Web/Tournaments/Domain/Tournament.cs` — Tournament document (TournamentPrices deeply nested — 7+ sub-objects)
  - `src/Web/Feeds/Domain/UserFeedItem.cs` — Feed item (indexed by Username)
  - `src/Web/Feeds/Domain/GlobalFeedItem.cs` — Global feed item (indexed by Id)
  - `src/Web/Feeds/Domain/ItemType.cs` — Feed item type enum
  - `src/Web/Leaderboard/HallOfFame.cs` — HallOfFame with inheritance hierarchy (AverageCourseHallOfFame, etc.)
  - `src/Web/Rounds/Domain/PlayerCourseStats.cs` — Player course statistics

  **External References**:
  - Npgsql EF Core provider docs: array mapping, JSONB columns, owned entities, xmin concurrency
  - EF Core global query filters documentation

  **WHY Each Reference Matters**:
  - Round.cs: Most complex mapping — 4-level nesting requires 6 tables with proper FK relationships
  - User.cs: Concurrency token, byte[] fields, polymorphic achievements collection
  - Tournament.cs: TournamentPrices must be JSONB column, not normalized (7+ nested types)
  - Achievements.cs: Reflection-based type discovery may conflict with EF Core proxies — test carefully
  - HallOfFame.cs: TPH inheritance requires discriminator column configuration
  - Course.cs: `Coordinates` record type needs `[Owned]` or `OwnsOne()` mapping

  **Acceptance Criteria**:

  - [ ] `DiscmanDbContext.cs` created with all DbSets
  - [ ] Entity configurations handle all edge cases (JSONB columns, arrays, owned entities, concurrency)
  - [ ] `dotnet ef migrations add InitialCreate` succeeds
  - [ ] `dotnet ef database update` creates all ~20 tables on a clean postgres:16

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: EF Core migration creates relational schema
    Tool: Bash
    Preconditions: Postgres 16 running in Docker, connection string set
    Steps:
      1. dotnet ef database update --project src/Web/Web.csproj
      2. Assert: Exit code 0, no errors
      3. docker exec postgres psql -U postgres -d discman -c "\dt"
      4. Assert: Tables exist for rounds, player_scores, hole_scores, stroke_specs, users, user_achievements, user_ratings, courses, course_holes, tournaments, global_feed_items, user_feed_items, hall_of_fame, reset_password_requests, player_course_stats
      5. docker exec postgres psql -U postgres -d discman -c "\d rounds"
      6. Assert: Columns include id, start_time, is_completed, deleted, course_name, etc.
    Expected Result: Full relational schema created from EF Core migrations
    Evidence: psql output captured

  Scenario: Dual registration - both Marten and EF Core registered
    Tool: Bash
    Preconditions: App builds, both registrations in Startup.cs
    Steps:
      1. dotnet build src/Web/Web.csproj
      2. Assert: No compilation errors about duplicate registrations
      3. Verify Startup.cs contains both services.ConfigureMarten() and services.AddDbContext<DiscmanDbContext>()
    Expected Result: Both ORMs registered without conflict
    Evidence: Build output + file inspection
  ```

  **Commit**: YES
  - Message: `feat: add EF Core DbContext with entity configurations and initial migration`
  - Files: `src/Web/Infrastructure/DiscmanDbContext.cs`, `src/Web/Infrastructure/EntityConfigurations/*.cs`, `src/Web/Migrations/`, `src/Web/Web.csproj`, `src/Web/Startup.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [x] 3. Update NServiceBus persistence to SQL persistence

  **What to do**:
  - Add `NServiceBus.Persistence.Sql` NuGet package
  - Update `NServiceBusConfiguration.cs` to use SQL persistence with PostgreSQL:
    ```csharp
    var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
    persistence.SqlDialect<SqlDialect.PostgreSql>();
    persistence.ConnectionBuilder(() => new NpgsqlConnection(connectionString));
    ```
  - Create the NServiceBus persistence tables (SQL persistence auto-creates or use installer)
  - Verify all `IHandleMessages<T>` handlers still work with the new persistence
  - Ensure the NServiceBus endpoint starts correctly with RabbitMQ 3.x transport

  **Must NOT do**:
  - Do NOT change business logic in message handlers
  - Do NOT drop RabbitMQ or change messaging patterns

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
    - Reason: NServiceBus configuration changes with breaking API between v8 and v10
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 2)
  - **Blocks**: Task 8
  - **Blocked By**: Task 1

  **References**:

  **Pattern References**:
  - `src/Web/Infrastructure/NServiceBusConfiguration.cs` — Current NServiceBus config (transport, endpoint name, serialization)
  - `src/Web/Program.cs` — Where NServiceBus is wired into the host

  **API/Type References**:
  - All `NSBEvents/` folders in each domain — IHandleMessages implementations
  - `src/Web/Rounds/NSBEvents/` — Round event handlers
  - `src/Web/Users/NSBEvents/` — User event handlers
  - `src/Web/Feeds/NSBEvents/` — Feed event handlers (if any)

  **External References**:
  - NServiceBus SQL Persistence docs: https://docs.particular.net/persistence/sql/
  - NServiceBus 10 upgrade guide: https://docs.particular.net/nservicebus/upgrades/

  **WHY Each Reference Matters**:
  - NServiceBusConfiguration.cs: Main file to modify — transport and persistence config
  - NSBEvents handlers: Must verify they compile with NServiceBus 10 handler interfaces
  - SQL Persistence docs: Needed for correct PostgreSQL dialect configuration

  **Acceptance Criteria**:

  - [ ] `NServiceBus.Persistence.Sql` package added
  - [ ] NServiceBusConfiguration uses SQL persistence with PostgreSQL dialect
  - [ ] App starts without NServiceBus errors
  - [ ] RabbitMQ queues created by NServiceBus endpoint

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: NServiceBus starts with SQL persistence
    Tool: Bash
    Preconditions: Postgres and RabbitMQ running in Docker, app built
    Steps:
      1. Start the web application with dotnet run
      2. Check logs for NServiceBus startup
      3. Assert: Logs contain "NServiceBus" and "started" (no "failed" or "error")
      4. docker exec rabbitmq rabbitmqctl list_queues
      5. Assert: At least one queue with the endpoint name exists
    Expected Result: NServiceBus endpoint running with SQL persistence
    Evidence: Application logs and rabbitmqctl output captured
  ```

  **Commit**: YES
  - Message: `feat: configure NServiceBus SQL persistence for PostgreSQL`
  - Files: `src/Web/Infrastructure/NServiceBusConfiguration.cs`, `src/Web/Web.csproj`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 4. Migrate Feeds + Leaderboard domains from Marten to EF Core

  **What to do**:
  - **GlobalFeedItem handlers** — Replace `IDocumentSession` with `DiscmanDbContext` in:
    - `src/Web/Feeds/Handlers/UpdateFeedsOnCompletedRound.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnScoreUpdated.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnRoundStarted.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnRoundDeleted.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnUserJoinedTournament.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnNewUserCreated.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnFriendsWasAdded.cs`
    - `src/Web/Feeds/Handlers/UpdateFeedsOnAchievementEarned.cs`
  - **UserFeedItem handlers** — Same pattern replacement
  - **StorageExtensions.cs** — Refactor `UpdateFriendsFeeds()` extension method from `IDocumentSession` to `DbContext`
  - **Feed queries** — Replace `_session.Query<GlobalFeedItem>()` / `_session.Query<UserFeedItem>()` with `_context.GlobalFeedItems` / `_context.UserFeedItems` LINQ queries
  - **ToggleLikeItem** — Update `src/Web/Feeds/Commands/ToggleLikeItem.cs` to use DbContext
  - **GetFeed** — Replace Marten query with EF Core query in `src/Web/Feeds/Queries/GetFeed.cs`
  - **HallOfFame** — Replace `_session.Query<HallOfFame>().SingleAsync()` with `_context.HallOfFame.SingleAsync()`
  - **GetLeaderboard** — Replace Marten aggregation with EF Core query (keep cache pattern)
  - **Pattern for replacement**: `_session.Store(x)` → `_context.Add(x)` or `_context.Update(x)`; `_session.SaveChangesAsync()` → `_context.SaveChangesAsync()`; `_session.Query<T>()` → `_context.Set<T>()`

  **Must NOT do**:
  - Do NOT refactor the feed data model (keep same behavior)
  - Do NOT add new feed features

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
    - Reason: Multiple files to modify systematically, but straightforward pattern replacement
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3 (with Tasks 5, 6)
  - **Blocks**: Task 8
  - **Blocked By**: Task 2

  **References**:

  **Pattern References**:
  - `src/Web/Feeds/Handlers/UpdateFeedsOnCompletedRound.cs` — Example of Store + SaveChanges pattern to replace
  - `src/Web/Feeds/StorageExtensions.cs` — IDocumentSession extension methods to convert
  - `src/Web/Feeds/Domain/UserFeedItem.cs` — Entity to map
  - `src/Web/Feeds/Domain/GlobalFeedItem.cs` — Entity to map
  - `src/Web/Leaderboard/Queries/GetHallOfFame.cs` — SingleAsync query pattern to replace
  - `src/Web/Infrastructure/DiscmanDbContext.cs` — DbContext to inject (created in Task 2)

  **WHY Each Reference Matters**:
  - Feed handlers: These are the files where IDocumentSession → DbContext swap happens
  - StorageExtensions: Extension method pattern needs rethinking for DbContext
  - Domain models: Need to verify EF Core entity config matches these classes

  **Acceptance Criteria**:

  - [ ] Zero `IDocumentSession` references in `src/Web/Feeds/` directory
  - [ ] Zero `IDocumentSession` references in `src/Web/Leaderboard/` directory
  - [ ] `dotnet build src/Web/Web.csproj` succeeds

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Feed domain has no Marten references
    Tool: Bash
    Preconditions: Migration complete
    Steps:
      1. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Feeds/
      2. Assert: No matches found (exit code 1)
      3. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Leaderboard/
      4. Assert: No matches found (exit code 1)
      5. dotnet build src/Web/Web.csproj
      6. Assert: Build succeeded
    Expected Result: All Marten references removed from Feeds and Leaderboard
    Evidence: grep and build output captured
  ```

  **Commit**: YES
  - Message: `refactor: migrate Feeds and Leaderboard domains from Marten to EF Core`
  - Files: `src/Web/Feeds/**/*.cs`, `src/Web/Leaderboard/**/*.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 5. Migrate Courses + Tournaments domains from Marten to EF Core

  **What to do**:
  - **Courses domain**:
    - `src/Web/Courses/Commands/CreateNewCourse.cs` — Replace Store + SaveChanges
    - `src/Web/Courses/Commands/UpdateCourse.cs` — Replace with Update + SaveChanges
    - `src/Web/Courses/Queries/GetCourses.cs` — Replace Marten Query with EF Core LINQ
    - `src/Web/Courses/UpdateCourseRatingsWorker.cs` — Replace `IDocumentStore.OpenSession()` with `IDbContextFactory<DiscmanDbContext>` or `IServiceScopeFactory`
    - Any Marten QueryExtensions for courses
  - **Tournaments domain**:
    - `src/Web/Tournaments/Commands/CreateTournament.cs` — Store → Add
    - `src/Web/Tournaments/Commands/AddPlayerToTournament.cs` — Load + Store → Find + Update
    - `src/Web/Tournaments/Commands/AddCourseToTournament.cs` — Same pattern
    - `src/Web/Tournaments/Commands/CalculatePrices.cs` — Prices stored as JSONB column
    - `src/Web/Tournaments/Queries/` — Replace Marten queries with EF Core
    - `src/Web/Tournaments/TournamentWorker.cs` — Replace IDocumentStore with IDbContextFactory
  - **Cache integration**: Verify CourseCache and TournamentCache still function (they likely just cache query results)

  **Must NOT do**:
  - Do NOT optimize course queries (some join with Rounds — these will break until Rounds are migrated, but since we have dual-registration, Round data still comes from Marten)
  - Do NOT change TournamentPrices structure (keep as JSONB)

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
    - Reason: Two domains, multiple files, worker pattern conversion
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3 (with Tasks 4, 6)
  - **Blocks**: Task 8
  - **Blocked By**: Task 2

  **References**:

  **Pattern References**:
  - `src/Web/Courses/Course.cs` — Course entity (Holes, Coordinates, Admins)
  - `src/Web/Courses/Commands/CreateNewCourse.cs` — Store pattern example
  - `src/Web/Courses/Commands/UpdateCourse.cs` — Update pattern example
  - `src/Web/Courses/UpdateCourseRatingsWorker.cs` — Background worker using IDocumentStore.OpenSession()
  - `src/Web/Tournaments/Domain/Tournament.cs` — Tournament entity (TournamentPrices nested object)
  - `src/Web/Tournaments/Commands/CreateTournament.cs` — Store pattern
  - `src/Web/Tournaments/TournamentWorker.cs` — Worker using IDocumentStore

  **WHY Each Reference Matters**:
  - Course.cs: Need to verify EF entity config matches (Coordinates as owned, Holes as child table, Admins as text[])
  - Workers: Important pattern change — IDocumentStore.OpenSession() → IDbContextFactory.CreateDbContextAsync()
  - Tournament.cs: TournamentPrices must remain JSONB — verify EF mapping handles this

  **Acceptance Criteria**:

  - [ ] Zero `IDocumentSession`/`IDocumentStore` references in `src/Web/Courses/` directory
  - [ ] Zero `IDocumentSession`/`IDocumentStore` references in `src/Web/Tournaments/` directory
  - [ ] `dotnet build src/Web/Web.csproj` succeeds

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Courses and Tournaments domains have no Marten references
    Tool: Bash
    Preconditions: Migration complete
    Steps:
      1. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Courses/
      2. Assert: No matches found
      3. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Tournaments/
      4. Assert: No matches found
      5. dotnet build src/Web/Web.csproj
      6. Assert: Build succeeded
    Expected Result: All Marten references removed from Courses and Tournaments
    Evidence: grep and build output captured
  ```

  **Commit**: YES
  - Message: `refactor: migrate Courses and Tournaments domains from Marten to EF Core`
  - Files: `src/Web/Courses/**/*.cs`, `src/Web/Tournaments/**/*.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 6. Migrate Users domain from Marten to EF Core

  **What to do**:
  - **User CRUD commands**:
    - `CreateNewUser.cs` — Store(newUser) → Add(newUser)
    - `UpdateUser.cs` / `SaveUser.cs` — Store → Update
    - `DeleteUser.cs` (if exists) — Delete pattern
  - **User query handlers**:
    - All queries using `_session.Query<User>()` → `_context.Users`
    - Handle `.Include()` for related data (Achievements, Ratings)
  - **Authentication**:
    - Login/auth commands — replace session Load/Query with DbContext Find/FirstOrDefault
    - Preserve JWT generation logic unchanged
    - Preserve byte[] Password and Salt mapping (these map to `bytea` columns in PostgreSQL)
  - **Workers**:
    - `ResetPasswordWorker.cs` — Replace IDocumentStore.OpenSession() with IDbContextFactory
    - `UserEmailNotificationWorker.cs` — Same pattern
  - **Achievements**:
    - The `Achievements` class implements `ICollection<Achievement>` with private `_achievements` backing field
    - Configure EF Core to access via backing field: `UsePropertyAccessMode(PropertyAccessMode.Field)`
    - Or refactor to use `public List<Achievement> Achievements { get; set; }` (simpler)
    - Achievement subclasses use TPH: add discriminator column in entity config
  - **Optimistic concurrency**:
    - Remove `[UseOptimisticConcurrency]` attribute from User.cs
    - Add `UseXminAsConcurrencyToken()` in EF Core entity configuration
  - **Friends list**: `List<string>` → map to `text[]` PostgreSQL array
  - **Rating history**: `List<Rating>` → separate `user_ratings` table or keep as JSONB column
  - **UsersController**: Has direct IDocumentSession injection — replace with DbContext

  **Must NOT do**:
  - Do NOT fix broken achievement evaluations (they have "logic does not work" comments)
  - Do NOT change password hashing logic
  - Do NOT alter JWT token format or claims

  **Recommended Agent Profile**:
  - **Category**: `ultrabrain`
    - Reason: Complex entity with concurrency tokens, polymorphic collections, byte[] mapping, private backing fields
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3 (with Tasks 4, 5)
  - **Blocks**: Task 7
  - **Blocked By**: Task 2

  **References**:

  **Pattern References**:
  - `src/Web/Users/Domain/User.cs` — Main entity with `[UseOptimisticConcurrency]`, byte[] Password/Salt, ICollection<Achievement>
  - `src/Web/Users/Domain/Achievements.cs` — Reflection-based achievement evaluation, ICollection with private backing field
  - `src/Web/Users/Domain/UserStats.cs` — Stats value object
  - `src/Web/Users/Domain/ResetPasswordRequest.cs` — Simple document
  - `src/Web/Users/Domain/PlayerBest.cs` — Player best scores
  - `src/Web/Users/Domain/SaltSeasonedHashedPassword.cs` — Password hashing (don't change)
  - `src/Web/Users/Commands/CreateNewUser.cs` — Store pattern
  - `src/Web/Users/UsersController.cs` — Direct IDocumentSession injection (17 files total in Users domain)
  - `src/Web/Users/ResetPasswordWorker.cs` — Worker using IDocumentStore
  - `src/Web/Users/UserEmailNotificationWorker.cs` — Worker using IDocumentStore

  **WHY Each Reference Matters**:
  - User.cs: Most complex User entity — concurrency, byte[] fields, achievements
  - Achievements.cs: Reflection + private backing field = EF Core mapping challenge
  - Workers: Pattern for IDocumentStore → IDbContextFactory conversion

  **Acceptance Criteria**:

  - [ ] Zero `IDocumentSession`/`IDocumentStore` references in `src/Web/Users/` directory
  - [ ] `[UseOptimisticConcurrency]` removed from User.cs
  - [ ] `UseXminAsConcurrencyToken()` configured in entity config
  - [ ] `dotnet build src/Web/Web.csproj` succeeds

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Users domain has no Marten references
    Tool: Bash
    Preconditions: Migration complete
    Steps:
      1. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession\|UseOptimisticConcurrency" src/Web/Users/
      2. Assert: No matches found
      3. dotnet build src/Web/Web.csproj
      4. Assert: Build succeeded
    Expected Result: All Marten references removed from Users domain
    Evidence: grep and build output captured
  ```

  **Commit**: YES
  - Message: `refactor: migrate Users domain from Marten to EF Core`
  - Files: `src/Web/Users/**/*.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 7. Migrate Rounds domain from Marten to EF Core

  **What to do**:
  - **This is the most complex domain** — Round has 4-level nesting that Marten loads as one document
  - **Round commands** (replace Store/Load/Query → Add/Find/Set):
    - `StartNewRoundCommand.cs` — Store(round) → Add(round) (with all nested entities)
    - `UpdatePlayerScore.cs` — HOT PATH: called on every stroke. Must Load round, update nested HoleScore, save. With EF Core: `Include()` chains to load nested graph, modify, SaveChanges with change tracking
    - `CompleteRound.cs` — Load + modify + save
    - `DeleteRound.cs` — Soft delete (set Deleted=true)
    - `LeaveRound.cs` — Remove player from round
    - `AddHole.cs` / `DeleteHole.cs` — Hole management commands
    - `SaveCourse.cs` (in Rounds/Commands) — Course save from rounds context
  - **Round queries** (replace Marten queries with EF Core LINQ):
    - All `_session.Query<Round>()` → `_context.Rounds.Include(r => r.PlayerScores).ThenInclude(ps => ps.HoleScores).ThenInclude(hs => hs.StrokeSpecs)` etc.
    - GetRoundPaceData, GetActiveRounds, GetCompletedRounds, etc.
  - **SignalR hub**:
    - `src/Web/Infrastructure/RoundsHub.cs` — Queries Marten directly. Replace with DbContext injection or MediatR queries
  - **Workers**:
    - `UpdateInActiveRoundsWorker.cs` — Replace IDocumentStore.OpenSession()
    - Any other round-related workers
  - **Related handlers**:
    - `NotifyPlayersOnRoundStarted.cs`, `NotifyPlayersOnRoundDeleted.cs` — Query users for round
    - Feed update handlers that query Round (may already be migrated in Task 4)
  - **Performance consideration**: `UpdatePlayerScore` is the hottest path. With Marten it loads the entire Round document. With EF Core, consider loading only the specific PlayerScore + HoleScores needed, or accept loading the full graph with Include chains

  **Must NOT do**:
  - Do NOT optimize the Round loading pattern (migrate as-is, optimize later)
  - Do NOT change the Round domain model structure
  - Do NOT fix or change soft-delete behavior

  **Recommended Agent Profile**:
  - **Category**: `ultrabrain`
    - Reason: Most complex domain — 4-level nesting, hot path performance, SignalR hub migration
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (Wave 4)
  - **Blocks**: Task 8
  - **Blocked By**: Task 6 (Users domain — shared queries reference both)

  **References**:

  **Pattern References**:
  - `src/Web/Rounds/Domain/Round.cs` — The big one: PlayerScores → HoleScores → StrokeSpecs (4 levels)
  - `src/Web/Rounds/Domain/PlayerCourseStats.cs` — Player stats document
  - `src/Web/Rounds/Commands/StartNewRoundCommand.cs` — Store(round) pattern
  - `src/Web/Rounds/Commands/UpdatePlayerScore.cs` — HOT PATH: Load + modify nested + save
  - `src/Web/Rounds/Commands/CompleteRound.cs` — Round completion handler
  - `src/Web/Rounds/Commands/DeleteRound.cs` — Soft delete
  - `src/Web/Rounds/Commands/LeaveRound.cs` — Player leaving round
  - `src/Web/Rounds/Commands/AddHole.cs` — Hole management
  - `src/Web/Rounds/Commands/DeleteHole.cs` — Hole management
  - `src/Web/Rounds/Commands/SaveCourse.cs` — Course save from rounds context
  - `src/Web/Rounds/Handlers/NotifyPlayersOnRoundStarted.cs` — Queries Round + User
  - `src/Web/Infrastructure/RoundsHub.cs` — Direct Marten session usage in SignalR hub
  - `src/Web/Rounds/Workers/` or root-level workers — Background workers

  **WHY Each Reference Matters**:
  - Round.cs: Core entity with deep nesting — defines the entire relational mapping challenge
  - UpdatePlayerScore.cs: Performance-critical path — how EF Core handles Include chains matters
  - RoundsHub.cs: SignalR hub injecting IDocumentSession directly — needs special attention for DI

  **Acceptance Criteria**:

  - [ ] Zero `IDocumentSession`/`IDocumentStore` references in `src/Web/Rounds/` directory
  - [ ] Zero `IDocumentSession` references in `src/Web/Infrastructure/RoundsHub.cs`
  - [ ] `dotnet build src/Web/Web.csproj` succeeds

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Rounds domain has no Marten references
    Tool: Bash
    Preconditions: Migration complete
    Steps:
      1. grep -r "IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/Rounds/
      2. Assert: No matches found
      3. grep -r "IDocumentSession\|IDocumentStore" src/Web/Infrastructure/RoundsHub.cs
      4. Assert: No matches found
      5. dotnet build src/Web/Web.csproj
      6. Assert: Build succeeded
    Expected Result: All Marten references removed from Rounds domain and SignalR hub
    Evidence: grep and build output captured
  ```

  **Commit**: YES
  - Message: `refactor: migrate Rounds domain from Marten to EF Core`
  - Files: `src/Web/Rounds/**/*.cs`, `src/Web/Infrastructure/RoundsHub.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 8. Remove Marten dependency and clean up Admin area

  **What to do**:
  - **Admin Razor Pages** — Migrate from IDocumentSession to DbContext:
    - `Admin/Rounds/Index.cshtml.cs` — List rounds query
    - `Admin/Rounds/RoundDetails.cshtml.cs` — Round detail view
    - `Admin/Users/Index.cshtml.cs` — List users query
    - `Admin/Users/Details.cshtml.cs` — User detail view
  - **Remove Marten completely**:
    - Delete `src/Web/Infrastructure/MartenConfiguration.cs`
    - Remove `services.ConfigureMarten()` call from Startup.cs
    - Remove Marten NuGet packages from Web.csproj: `Marten`, `Weasel.Postgresql`, any Marten-related packages
    - Remove all remaining `using Marten;` statements across the codebase
    - Remove `[UseOptimisticConcurrency]` attribute if still present
  - **Final verification**: Grep entire codebase for any remaining Marten references
  - **Clean up env var naming**: Ensure DOTNET_POSTGRES_CON_STRING is used consistently (not POSTGRES_CON_STRING which was Marten-specific in MartenConfiguration.cs)

  **Must NOT do**:
  - Do NOT change Admin auth logic (keep cookie-based JWT with "kofoed" admin check)
  - Do NOT add new admin features

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Straightforward cleanup — delete config, remove packages, migrate 4 Razor Pages
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (Wave 5)
  - **Blocks**: Task 9
  - **Blocked By**: Tasks 4, 5, 6, 7, 3

  **References**:

  **Pattern References**:
  - `src/Web/Infrastructure/MartenConfiguration.cs` — File to DELETE
  - `src/Web/Startup.cs` — Remove ConfigureMarten() call
  - `src/Web/Web.csproj` — Remove Marten package references
  - `src/Web/Admin/` — Razor Pages to migrate (4 files)

  **WHY Each Reference Matters**:
  - MartenConfiguration.cs: Main deletion target
  - Startup.cs: Must remove Marten registration without breaking EF Core registration
  - Admin pages: Last remaining IDocumentSession users

  **Acceptance Criteria**:

  - [ ] `MartenConfiguration.cs` deleted
  - [ ] Zero references to `Marten`, `IDocumentSession`, `IDocumentStore`, `IQuerySession` in entire `src/Web/` directory
  - [ ] Marten NuGet packages removed from Web.csproj
  - [ ] `dotnet build src/Web/Web.csproj` succeeds

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: Zero Marten references in entire codebase
    Tool: Bash
    Preconditions: All domain migrations complete
    Steps:
      1. grep -r "Marten\|IDocumentSession\|IDocumentStore\|IQuerySession" src/Web/ --include="*.cs"
      2. Assert: No matches found (exit code 1)
      3. grep -r "Marten" src/Web/Web.csproj
      4. Assert: No matches found
      5. dotnet build src/Web/Web.csproj
      6. Assert: Build succeeded
      7. dotnet publish src/Web/Web.csproj -o /tmp/discman-clean
      8. Assert: Publish succeeded
    Expected Result: Marten completely removed, app builds and publishes
    Evidence: grep and build output captured
  ```

  **Commit**: YES
  - Message: `refactor: remove Marten dependency and migrate Admin area to EF Core`
  - Files: `src/Web/Infrastructure/MartenConfiguration.cs` (deleted), `src/Web/Startup.cs`, `src/Web/Web.csproj`, `src/Web/Admin/**/*.cs`
  - Pre-commit: `dotnet build src/Web/Web.csproj`

---

- [ ] 9. Create SQL ETL scripts for data migration (user-executed)

  **What to do**:
  - **Create ETL SQL scripts** that transform Marten JSONB → relational tables
    - Scripts will be run by the user manually after restoring a production pg_dump into their local postgres
    - Script per domain, reading from `disclive_production.mt_doc_*` tables:
    - `etl_users.sql`: Extract from `mt_doc_user` → INSERT into users, user_achievements, user_ratings, user_friends
    - `etl_courses.sql`: Extract from `mt_doc_course` → INSERT into courses, course_holes
    - `etl_rounds.sql`: Most complex — `mt_doc_round` → rounds, player_scores (jsonb_array_elements), hole_scores, stroke_specs, player_signatures, rating_changes
    - `etl_tournaments.sql`: `mt_doc_tournament` → tournaments (with TournamentPrices as JSONB column)
    - `etl_feeds.sql`: `mt_doc_globalfeeditem` and `mt_doc_userfeeditem` → feed tables
    - `etl_halloffame.sql`: `mt_doc_halloffame` → hall_of_fame
    - `etl_misc.sql`: ResetPasswordRequests, PlayerCourseStats
  - **Create a master `run_etl.sh` script** that runs all ETL scripts in the correct order (users before rounds, etc.) and outputs row count comparisons
  - **Create a `README_ETL.md`** documenting:
    - Prerequisites (postgres:16+, pg_dump from production)
    - How to restore the production dump
    - How to run the ETL scripts
    - How to verify the data migration
    - Expected row counts format
  - **Verification queries**: Include verification SQL at the end of each script that compares source document count vs target row count

  **Must NOT do**:
  - Do NOT run the scripts against production — user does that
  - Do NOT drop Marten tables in the scripts (user decides when to clean up)
  - Do NOT modify production data on Digital Ocean

  **Recommended Agent Profile**:
  - **Category**: `ultrabrain`
    - Reason: Complex SQL ETL with JSONB extraction, multi-level jsonb_array_elements, data integrity verification
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (Wave 6)
  - **Blocks**: None
  - **Blocked By**: Task 8

  **References**:

  **Pattern References**:
  - `src/Web/Infrastructure/MartenConfiguration.cs` — Schema name pattern: `disclive_{EnvironmentName}` (so production is `disclive_Production` or `disclive_production`)
  - `src/Web/Rounds/Domain/Round.cs` — Round structure for ETL script design (PlayerScores → HoleScores → StrokeSpecs nesting)
  - `src/Web/Users/Domain/User.cs` — User structure (byte[] Password/Salt, Achievements, Friends)
  - `src/Web/Tournaments/Domain/Tournament.cs` — TournamentPrices nested structure (keep as JSONB)
  - `README.md` — Backup/restore commands (pg_dumpall, psql restore)

  **WHY Each Reference Matters**:
  - MartenConfiguration: Need exact schema name to query mt_doc_* tables
  - Round.cs: Most complex ETL — must understand nesting to write jsonb_array_elements chains
  - User.cs: byte[] fields need special handling in JSONB → bytea conversion
  - README: Has existing backup/restore patterns to follow

  **Acceptance Criteria**:

  - [ ] ETL scripts created for all 7+ document types in `infrastructure/etl/` directory
  - [ ] `run_etl.sh` master script created
  - [ ] `README_ETL.md` created with clear instructions
  - [ ] Each ETL script includes verification queries (source count vs target count)
  - [ ] `dotnet build src/Web/Web.csproj` still succeeds (scripts don't break the build)

  **Agent-Executed QA Scenarios:**

  ```
  Scenario: ETL scripts are syntactically valid SQL
    Tool: Bash
    Preconditions: Postgres running in Docker, EF Core schema applied
    Steps:
      1. For each .sql file in infrastructure/etl/:
         docker exec -i postgres psql -U postgres -d discman -f - < infrastructure/etl/<file> --set ON_ERROR_STOP=1 2>&1
         (Run against empty EF Core schema — INSERT 0 rows is expected, but syntax must be valid)
      2. Assert: No SQL syntax errors for any script
      3. Verify run_etl.sh is executable: test -x infrastructure/etl/run_etl.sh
      4. Assert: Script is executable
      5. Verify README_ETL.md exists and contains key sections:
         grep -c "Prerequisites\|Restore\|Run\|Verify" infrastructure/etl/README_ETL.md
      6. Assert: At least 4 matches (all key sections present)
    Expected Result: All ETL scripts are valid SQL, master script is executable, documentation exists
    Evidence: psql output for each script captured
  ```

  **Commit**: YES
  - Message: `data: add SQL ETL scripts for Marten JSONB to relational table migration`
  - Files: `infrastructure/etl/*.sql`, `infrastructure/etl/run_etl.sh`, `infrastructure/etl/README_ETL.md`
  - Pre-commit: N/A

---

## Commit Strategy

| After Task | Message | Key Files | Verification |
|------------|---------|-----------|--------------|
| 1 | `chore: upgrade to .NET 10 and update all major dependencies` | Web.csproj, Dockerfile, Startup.cs | dotnet build |
| 2 | `feat: add EF Core DbContext with entity configurations and initial migration` | DiscmanDbContext.cs, Migrations/, Startup.cs | dotnet build |
| 3 | `feat: configure NServiceBus SQL persistence for PostgreSQL` | NServiceBusConfiguration.cs | dotnet build |
| 4 | `refactor: migrate Feeds and Leaderboard domains from Marten to EF Core` | Feeds/**/*.cs, Leaderboard/**/*.cs | dotnet build |
| 5 | `refactor: migrate Courses and Tournaments domains from Marten to EF Core` | Courses/**/*.cs, Tournaments/**/*.cs | dotnet build |
| 6 | `refactor: migrate Users domain from Marten to EF Core` | Users/**/*.cs | dotnet build |
| 7 | `refactor: migrate Rounds domain from Marten to EF Core` | Rounds/**/*.cs, RoundsHub.cs | dotnet build |
| 8 | `refactor: remove Marten dependency and migrate Admin area to EF Core` | MartenConfiguration.cs (deleted), Web.csproj | dotnet build |
| 9 | `data: add SQL ETL scripts for Marten JSONB to relational table migration` | infrastructure/etl/*.sql | N/A |

---

## Success Criteria

### Verification Commands
```bash
# App builds on .NET 10
dotnet build src/Web/Web.csproj  # Expected: Build succeeded

# Zero Marten references
grep -r "Marten\|IDocumentSession\|IDocumentStore" src/Web/ --include="*.cs"  # Expected: no output

# Docker image builds
docker build -t discman:latest -f src/Web/Dockerfile .  # Expected: exit code 0

# EF Core migration applies
dotnet ef database update --project src/Web/Web.csproj  # Expected: tables created

# ETL scripts exist
ls infrastructure/etl/*.sql  # Expected: 7+ .sql files
```

### Final Checklist
- [ ] All "Must Have" present
- [ ] All "Must NOT Have" absent (no Marten, no ELK, no test suite, no broken achievement fixes)
- [ ] App starts locally with `dotnet watch run` and connects to Docker postgres + rabbitmq
- [ ] SQL ETL scripts created and documented
- [ ] Old Digital Ocean production still running untouched
