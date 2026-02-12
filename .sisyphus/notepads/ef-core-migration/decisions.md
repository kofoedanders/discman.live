
## 2026-02-12 16:25 - Entity Configurations Created (8 entities)

All EF Core IEntityTypeConfiguration classes created in `src/Web/Infrastructure/EntityConfigurations/`:

### 1. ResetPasswordRequestConfiguration
- Simple table: `reset_password_requests`
- All properties required

### 2. UserFeedItemConfiguration
- Simple table: `user_feed_items`
- All properties required
- ItemType enum stored as integer (EF default)

### 3. GlobalFeedItemConfiguration
- Table: `global_feed_items`
- **PostgreSQL arrays**: Subjects, Likes (text[]), RoundScores (integer[])

### 4. HallOfFameConfiguration
- Table: `hall_of_fames`
- **TPH inheritance**: Discriminator column `hall_of_fame_type` ("base" | "month")
- **Owned entities**: MostBirdies, MostBogies, MostRounds, BestRoundAverage (embedded)
- MonthHallOfFame properties: Month, Year, CreatedAt (shadow properties)
- Computed property ignored: `DaysInHallOfFame`

### 5. TournamentConfiguration
- Table: `tournaments`
- **PostgreSQL arrays**: Players, Admins (text[]), Courses (uuid[])
- **JSONB column**: TournamentPrices (complex nested structure with 7+ types)

### 6. CourseConfiguration
- Table: `courses`
- **Owned entity (embedded)**: Coordinates record (Latitude, Longitude) with precision(9,6)
- **Child table**: Holes → `holes` table with FK CourseId
- Admins as text[]
- Computed property ignored: `CourseAverageScore`

### 7. UserConfiguration
- Table: `users`
- **Concurrency**: TODO - requires xmin system column or version property (not implemented yet)
- **PostgreSQL arrays**: Friends, NewsIdsSeen (text[])
- **Child table**: RatingHistory → `user_rating_history` table
- **JSONB column**: Achievements (custom collection type Achievements, not mappable as owned)
  - Originally planned as TPH with 15 discriminators, but Achievements is ICollection<Achievement> wrapper
  - Stored as JSONB for now (Marten compatibility)

### 8. RoundConfiguration (MOST COMPLEX - 6 tables)
- Main table: `rounds`
- **Soft-delete**: Query filter `.HasQueryFilter(r => !r.Deleted)`
- Computed property ignored: `DurationMinutes`
- **PostgreSQL array**: Spectators (text[])
- **JSONB column**: Achievements (base Achievement instances)
- **6 child tables**:
  1. `player_signatures` (PlayerSignature owned collection)
  2. `rating_changes` (RatingChange owned collection)
  3. `player_scores` (PlayerScore owned collection)
  4. `hole_scores` (HoleScore nested inside PlayerScore)
  5. Hole embedded in hole_scores as owned entity (HoleNumber, HolePar, HoleDistance, HoleAverage, HoleRating columns)
  6. `stroke_specs` (StrokeSpec nested inside HoleScore)

### Key Mapping Decisions:

1. **Enums**: ScoreMode, ItemType, Action, StrokeOutcome → stored as integers (EF default)
2. **PostgreSQL arrays**: Used `HasColumnType("text[]")`, `HasColumnType("integer[]")`, `HasColumnType("uuid[]")`
3. **JSONB columns**: Used `HasColumnType("jsonb")` for complex types (TournamentPrices, Achievements, Round.Achievements)
4. **Owned collections**: Use `.OwnsMany()` with shadow FK and synthetic Id column
5. **Embedded owned entities**: Use `.OwnsOne()` for value objects (Coordinates, Hole in hole_scores)
6. **TPH inheritance**: HallOfFame with discriminator `hall_of_fame_type`
7. **Computed properties**: Ignored with `.Ignore()` (DurationMinutes, CourseAverageScore, DaysInHallOfFame, AchievementName)
8. **Soft-delete**: Query filter on Round entity
9. **Table names**: All snake_case (rounds, users, courses, etc.)

### Build Status: ✅ SUCCESS
- Zero LSP errors
- `dotnet build` passes with only pre-existing warnings
- All 8 configurations registered via `ApplyConfigurationsFromAssembly()`

## 2026-02-12 - RoundConfiguration completed

- Implemented nested OwnsMany/OwnsOne chain for Round → PlayerScores → HoleScores → (Hole embedded + StrokeSpecs).
- Used JSONB for Round.Achievements per requirement; Spectators stored as text[] array.
