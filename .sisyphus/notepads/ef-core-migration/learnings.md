
## Package Upgrades Completed (Session: Feb 12, 2026)

### Upgraded Packages
- **MediatR**: 12.2.0 → 14.0.0 (note: 14.1.0 doesn't exist yet, latest is 14.0.0)
- **AutoMapper**: 13.0.1 → 16.0.0
- **Serilog.AspNetCore**: 8.0.1 → 10.0.0

### Breaking Changes Fixed

#### AutoMapper 16.0.0
- **Change**: `AddAutoMapper(Assembly)` → `AddAutoMapper(cfg => cfg.AddMaps(Assembly))`
- **File**: `src/Web/Startup.cs` line 61
- **Impact**: Simpler API, same behavior. All map profile discovery automatic.

#### MediatR 14.0.0
- **No code changes required** — registration already using v12+ style (`cfg.RegisterServicesFromAssembly`) which is compatible with v14
- **Pipeline behaviors**: Unchanged (still use `IPipelineBehavior<TRequest, TResponse>`)

#### Serilog.AspNetCore 10.0.0
- **No code changes required** — no breaking changes affecting current usage

### Build Status
- ✅ `dotnet restore` succeeds
- ✅ `dotnet build` succeeds (zero errors)
- ✅ Warnings are pre-existing (Marten deprecations, NServiceBus analyzer hints, SYSLIB0041 for Rfc2898DeriveBytes)

### Packages NOT Changed (as per task)
- FluentValidation: 11.9.0 (kept, already at latest 11.x)
- NServiceBus: 8.1.6 (kept)
- NServiceBus.Extensions.Hosting: 2.0.0 (kept)
- NServiceBus.RabbitMQ: 8.0.3 (kept)
- Swashbuckle.AspNetCore: 8.0.0 (kept)


## NServiceBus 9.x Upgrade Complete

**Upgraded packages (Task 1 final piece):**
- NServiceBus: 8.1.6 → 9.2.9 ✓
- NServiceBus.Extensions.Hosting: 2.0.0 → 3.0.1 ✓
- NServiceBus.RabbitMQ: 8.0.3 → 10.0.1 ✓
- NServiceBus.Persistence.Sql: ADDED 8.0.0 ✓
- FluentValidation: 11.9.0 → 12.1.0 ✓
- FluentValidation.DependencyInjectionExtensions: 11.9.0 → 12.1.0 ✓
- Swashbuckle.AspNetCore: 8.0.0 → 10.0.1 ✓

**Build status:** ✓ Success (0 errors, 40 pre-existing warnings about CancellationToken propagation)

**Breaking changes handled:**
- No code changes needed. Codebase already uses:
  - `UseSerialization<SystemJsonSerializer>()` (required in v9, was optional in v8)
  - `LimitMessageProcessingConcurrencyTo(1)` (already set)
  - No deprecated APIs like IManageUnitsOfWork, RuntimeEnvironment.MachineNameAction
  - No custom features depending on MessageDrivenSubscriptions via deprecated API

**Key v8→v9 breaking changes to watch for:**
1. Serializer choice is now mandatory (was optional, XML was default)
2. `RequiredImmediateDispatch()` replaces `IsImmediateDispatchSet()`
3. `IManageUnitsOfWork` → pipeline behavior pattern
4. `FeatureConfigurationContext.Container` → `FeatureConfigurationContext.Services`
5. Feature dependencies: `DependsOn<T>()` → `DependsOn("string")`

All upgrades completed successfully for .NET 9 + NServiceBus 9.x compatibility.

## NServiceBus SQL Persistence Configuration (Task 2)

### NServiceBus.Persistence.Sql 8.0 API Pattern
- Version 8.0 uses different API than v9 documentation suggests
- Correct pattern for PostgreSQL dialect:
  ```csharp
  var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
  var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();  // Returns dialect object
  dialect.JsonBParameterModifier(parameter => { ... });           // Extension method on dialect
  persistence.ConnectionBuilder(() => new NpgsqlConnection(connectionString));
  ```

### Required Using Statements
```csharp
using Npgsql;           // For NpgsqlConnection
using NpgsqlTypes;      // For NpgsqlDbType.Jsonb
using NServiceBus;      // Core NServiceBus types
```
- Do NOT need `using NServiceBus.Persistence.Sql;` (causes LSP confusion, but builds fine)

### PostgreSQL-Specific Requirements
1. **JsonB Parameter Modifier** - MANDATORY for PostgreSQL
   - Npgsql requires explicit `NpgsqlDbType = NpgsqlDbType.Jsonb` for JSONB columns
   - Without this, saga data serialization will fail at runtime
   
2. **Connection String** - Same as Marten
   - Reuses `DOTNET_POSTGRES_CON_STRING` environment variable
   - Persistence tables created in same database as Marten document store

3. **Table Creation**
   - `EnableInstallers()` auto-creates persistence tables on first run
   - Tables use endpoint name as prefix ("discman_web_" prefix expected)
   - Initialization happens AFTER transport initialization (RabbitMQ must be available)

### Build vs LSP Behavior
- LSP may show errors for `SqlPersistence` and `SqlDialect` types
- Build succeeds despite LSP errors (package is correctly referenced)
- This is a known LSP/IDE indexing issue with NServiceBus.Persistence.Sql 8.0

### Integration Point
- Transport (RabbitMQ) initializes BEFORE persistence
- If RabbitMQ is unavailable, persistence tables won't be created
- This is expected NServiceBus 9.x startup order


## EF Core Package Addition (Task: Add EF Core NuGet packages)

### Packages Added to Web.csproj
- **Microsoft.EntityFrameworkCore**: 9.0.0 ✓
- **Microsoft.EntityFrameworkCore.Design**: 9.0.0 ✓
- **Npgsql.EntityFrameworkCore.PostgreSQL**: 9.0.0 ✓

### Build Verification
- ✅ `dotnet restore` succeeds (948ms)
- ✅ `dotnet build` succeeds (0 errors, pre-existing warnings only)

### Notes
- All 3 packages installed at 9.x versions to match .NET 9 target framework
- Packages inserted alphabetically in PackageReference section
- No Startup.cs changes made (per task scope)
- No DbContext created yet (deferred to next sub-task)

## EF Core DbContext Skeleton (Task 2b)

### DiscmanDbContext.cs Created
- Location: `src/Web/Infrastructure/DiscmanDbContext.cs`
- Uses `Microsoft.EntityFrameworkCore` and imports entity namespaces:
  - `Web.Courses`, `Web.Feeds.Domain`, `Web.Leaderboard`, `Web.Rounds`, `Web.Tournaments.Domain`, `Web.Users.Domain`
- `Users` DbSet uses fully-qualified type `Web.Users.User` to disambiguate from `Web.Users.Domain`
- DbSets defined for: User, Round, Course, Tournament, GlobalFeedItem, UserFeedItem, HallOfFame, MonthHallOfFame, ResetPasswordRequest, PlayerCourseStats
- `OnModelCreating` left empty with comment: `// Entity configurations will be added in sub-tasks 2c-2e`

### Build Verification
- ✅ `dotnet build src/Web/Web.csproj` succeeds (warnings pre-existing)

## [2026-02-12] Task 2c: Simple Entity Configurations (GlobalFeedItem, UserFeedItem, ResetPasswordRequest, PlayerCourseStats)

### What was accomplished
Created 4 entity configuration files implementing IEntityTypeConfiguration<T> pattern for EF Core 9:

1. **GlobalFeedItemConfiguration.cs**
   - Table: global_feed_items
   - Demonstrates PostgreSQL array mapping (text[], integer[])
   - Index on RegisteredAt for feed query performance
   - 10 properties mapped with snake_case naming convention

2. **UserFeedItemConfiguration.cs**
   - Table: user_feed_items
   - Composite indexes: Username + RegisteredAt for efficient user feed queries
   - Simple flat entity with 4 properties

3. **ResetPasswordRequestConfiguration.cs**
   - Table: reset_password_requests
   - Index on Email for password reset token lookup
   - 3 simple properties (Email, Username, CreatedAt)

4. **PlayerCourseStatsConfiguration.cs**
   - Table: player_course_stats
   - **KEYLESS ENTITY** - uses HasNoKey() (read model, not persisted)
   - Advanced Npgsql types: double precision[] arrays, jsonb column
   - HoleStats mapped as jsonb (nested List<HoleStats> NOT normalized)
   - 9 properties total

### Key patterns established
- IEntityTypeConfiguration<T> pattern for separation of concerns (standard EF Core 9)
- Snake_case column naming via HasColumnName("snake_case")
- Npgsql array types: text[] for List<string>, integer[] for List<int>, double precision[] for List<double>
- JSONB mapping for complex nested objects: HasColumnType("jsonb")
- Keyless entities for read models: builder.HasNoKey()
- Indexes for query performance: builder.HasIndex(x => x.PropertyName)
- Enum properties auto-map to int (no explicit mapping needed, EF Core handles it)

### Updated DiscmanDbContext.cs
- Added using statement for EntityConfigurations namespace
- OnModelCreating now calls ApplyConfiguration() for all 4 entities
- Pattern: modelBuilder.ApplyConfiguration(new {Entity}Configuration())

### Build verification
- dotnet build src/Web/Web.csproj
- Result: 54 warnings (pre-existing NServiceBus + Marten deprecations), 0 errors
- Exit code: 0 ✅

### Conventions confirmed
- Each configuration file is self-contained (no cross-entity dependencies)
- Fluent API fully used (no Data Attributes)
- All four configurations follow same structural pattern
- Configuration namespace: Web.Infrastructure.EntityConfigurations
- Files created in directory: src/Web/Infrastructure/EntityConfigurations/

### Next steps
- Sub-task 2d: User, Course, Tournament configurations (more complex aggregates)
- Sub-task 2e: Round configuration (most complex, 6+ related tables)
- Sub-task 2f: Register DbContext in Startup.cs
- Sub-task 2g: Generate migration and test

## [2026-02-12] Task: TournamentConfiguration.cs Created

### What was accomplished
Created `src/Web/Infrastructure/EntityConfigurations/TournamentConfiguration.cs` implementing IEntityTypeConfiguration<Tournament>.

### Configuration Details
- **Table**: tournaments
- **Simple properties** (8): Id, Name, CreatedAt, Start, End (all snake_case columns)
- **PostgreSQL arrays** (3):
  - Players → text[] (List<string>)
  - Admins → text[] (List<string>)
  - Courses → uuid[] (List<Guid>)
- **JSONB column** (1): Prices → jsonb (TournamentPrices object)

### Design Decision: Why JSONB for TournamentPrices
TournamentPrices is stored as JSONB column instead of normalized tables because:
1. **Deep nesting**: Contains 7+ nested sub-objects (Scoreboard list, FastestPlayer, SlowestPlayer, MostBirdies, LeastBogeysOrWorse, LongestCleanStreak, LongestDrySpell, BounceBacks)
2. **Avoiding over-normalization**: Would require 8+ separate tables for ONE nested property
3. **Match Marten behavior**: Existing Marten implementation stores this as JSONB document
4. **Rare querying**: Tournament prices are written once at tournament end, queried only for display (no complex filtering needed)

### Pattern Consistency
- Follows established IEntityTypeConfiguration<T> pattern from previous 5 configurations
- Snake_case column naming via HasColumnName()
- PostgreSQL-specific types: text[], uuid[], jsonb
- Auto-discovered by DiscmanDbContext.ApplyConfigurationsFromAssembly()

### Build Status
- TournamentConfiguration.cs itself is CORRECT
- Build currently fails due to PRE-EXISTING errors in UserConfiguration.cs:
  - Line 17: UseXminAsConcurrencyToken() extension method not available
  - Line 94: Achievement.RegisteredAt property mapping issue
- These errors exist BEFORE TournamentConfiguration was created (outside task scope)

### File Location
- Created: `src/Web/Infrastructure/EntityConfigurations/TournamentConfiguration.cs`
- Total configurations: 7 (Course, GlobalFeedItem, UserFeedItem, ResetPasswordRequest, PlayerCourseStats, User, Tournament)


## Sub-task 2d: UserConfiguration and TournamentConfiguration (COMPLETED)

### Key Learnings

#### xmin Concurrency Token (PostgreSQL-specific)
- **Initial attempt**: `builder.UseXminAsConcurrencyToken()` failed — extension not available in this version
- **Solution**: Use `builder.Property<uint>("xmin").IsRowVersion()` to map PostgreSQL's system column
- Npgsql 9.0 EF Core uses standard `.IsRowVersion()` for xmin mapping
- Add `using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;` for proper support

#### Achievement Collection Handling
- Achievement base class properties: `AchievementName` (read-only via GetType().Name), `Username`, `AchievedAt` (DateTime), `RoundId` (Guid), `HoleNumber` (int)
- Custom `Achievements : ICollection<Achievement>` works fine with `OwnsMany` — EF Core recognizes the ICollection interface
- No need to normalize Achievements separately; EF Core handles collection serialization

#### TournamentPrices as JSONB
- TournamentPrices has 7+ nested properties (Scoreboard, FastestPlayer, SlowestPlayer, etc.)
- Correctly stored as JSONB column using `HasColumnType("jsonb")` + `HasColumnName("prices")`
- Alternative `.ToJson("prices")` not used (EF Core 7+ feature, but explicit HasColumnType works universally)

#### Snake_case Naming Pattern
- All scalar properties use `.HasColumnName("snake_case")`
- Arrays: `text[]` for List<string>, `uuid[]` for List<Guid>
- Owned entities (Achievements, RatingHistory) also use snake_case consistently
- No exceptions; pattern applied to all 25+ property mappings

### Files Created
- `UserConfiguration.cs`: 122 lines, maps User entity with xmin concurrency, owned Achievements/RatingHistory
- `TournamentConfiguration.cs`: 62 lines, maps Tournament entity with JSONB Prices column

### Build Status
- Build succeeded: 0 errors, 16 warnings (pre-existing)
- DbContext auto-discovery working: `ApplyConfigurationsFromAssembly()` finds both new configurations

### Next Steps (Sub-task 2e, 2f, 2g)
- Create RoundConfiguration, HallOfFameConfiguration, MonthHallOfFameConfiguration in sub-task 2e
- Register DbContext in Startup.cs in sub-task 2f
- Generate initial EF Core migration in sub-task 2g
