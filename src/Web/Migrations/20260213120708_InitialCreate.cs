using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Web.Rounds.Queries;
using Web.Tournaments.Domain;
using Web.Users;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    layout = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    admins = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "global_feed_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    achievement_name = table.Column<string>(type: "text", nullable: true),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subjects = table.Column<List<string>>(type: "text[]", nullable: true),
                    course_name = table.Column<string>(type: "text", nullable: true),
                    hole_score = table.Column<int>(type: "integer", nullable: false),
                    hole_number = table.Column<int>(type: "integer", nullable: false),
                    round_scores = table.Column<List<int>>(type: "integer[]", nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    likes = table.Column<List<string>>(type: "text[]", nullable: true),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tournament_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tournament_name = table.Column<string>(type: "text", nullable: true),
                    friend_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_feed_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hall_of_fames",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    most_birdies_count = table.Column<int>(type: "integer", nullable: true),
                    most_birdies_per_round = table.Column<double>(type: "double precision", nullable: true),
                    most_birdies_username = table.Column<string>(type: "text", nullable: true),
                    most_birdies_time_of_entry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    most_birdies_new_this_month = table.Column<bool>(type: "boolean", nullable: true),
                    most_bogies_count = table.Column<int>(type: "integer", nullable: true),
                    most_bogies_per_round = table.Column<double>(type: "double precision", nullable: true),
                    most_bogies_username = table.Column<string>(type: "text", nullable: true),
                    most_bogies_time_of_entry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    most_bogies_new_this_month = table.Column<bool>(type: "boolean", nullable: true),
                    most_rounds_count = table.Column<int>(type: "integer", nullable: true),
                    most_rounds_username = table.Column<string>(type: "text", nullable: true),
                    most_rounds_time_of_entry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    most_rounds_new_this_month = table.Column<bool>(type: "boolean", nullable: true),
                    best_round_average_round_average = table.Column<double>(type: "double precision", nullable: true),
                    best_round_average_username = table.Column<string>(type: "text", nullable: true),
                    best_round_average_time_of_entry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    best_round_average_new_this_month = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    hall_of_fame_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hall_of_fames", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_course_stats",
                columns: table => new
                {
                    course_name = table.Column<string>(type: "text", nullable: true),
                    layout_name = table.Column<string>(type: "text", nullable: true),
                    player_name = table.Column<string>(type: "text", nullable: true),
                    course_average = table.Column<double>(type: "double precision", nullable: false),
                    player_course_record = table.Column<int>(type: "integer", nullable: true),
                    this_round_vs_average = table.Column<double>(type: "double precision", nullable: false),
                    hole_averages = table.Column<List<double>>(type: "double precision[]", nullable: true),
                    average_prediction = table.Column<List<double>>(type: "double precision[]", nullable: true),
                    rounds_played = table.Column<int>(type: "integer", nullable: false),
                    hole_stats = table.Column<List<HoleStats>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "reset_password_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reset_password_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spectators = table.Column<List<string>>(type: "text[]", nullable: false),
                    score_mode = table.Column<int>(type: "integer", nullable: false),
                    round_name = table.Column<string>(type: "text", nullable: true),
                    course_name = table.Column<string>(type: "text", nullable: true),
                    course_layout = table.Column<string>(type: "text", nullable: true),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    achievements = table.Column<List<Achievement>>(type: "jsonb", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rounds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    players = table.Column<List<string>>(type: "text[]", nullable: false),
                    admins = table.Column<List<string>>(type: "text[]", nullable: false),
                    start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    courses = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    prices = table.Column<TournamentPrices>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_feed_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    feed_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_feed_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<byte[]>(type: "bytea", nullable: false),
                    salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    friends = table.Column<List<string>>(type: "text[]", nullable: false),
                    discman_points = table.Column<int>(type: "integer", nullable: false),
                    elo = table.Column<double>(type: "double precision", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    simple_scoring = table.Column<bool>(type: "boolean", nullable: false),
                    emoji = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    register_put_distance = table.Column<bool>(type: "boolean", nullable: false),
                    news_ids_seen = table.Column<List<string>>(type: "text[]", nullable: false),
                    settings_initialized = table.Column<bool>(type: "boolean", nullable: false),
                    last_email_sent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "course_holes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<int>(type: "integer", nullable: false),
                    par = table.Column<int>(type: "integer", nullable: false),
                    distance = table.Column<int>(type: "integer", nullable: false),
                    average = table.Column<double>(type: "double precision", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_holes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_course_holes_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_scores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_name = table.Column<string>(type: "text", nullable: false),
                    player_emoji = table.Column<string>(type: "text", nullable: true),
                    player_round_status_emoji = table.Column<string>(type: "text", nullable: true),
                    course_average_at_the_time = table.Column<double>(type: "double precision", nullable: false),
                    number_of_hcp_strokes = table.Column<int>(type: "integer", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_scores_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    base64_signature = table.Column<string>(type: "text", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_signatures_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rating_changes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    change = table.Column<double>(type: "double precision", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rating_changes_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    elo = table.Column<double>(type: "double precision", nullable: false),
                    date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_ratings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hole_scores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hole_number = table.Column<int>(type: "integer", nullable: true),
                    hole_par = table.Column<int>(type: "integer", nullable: true),
                    hole_distance = table.Column<int>(type: "integer", nullable: true),
                    hole_average = table.Column<double>(type: "double precision", nullable: true),
                    hole_rating = table.Column<int>(type: "integer", nullable: true),
                    strokes = table.Column<int>(type: "integer", nullable: false),
                    relative_to_par = table.Column<int>(type: "integer", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayerScoreId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hole_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hole_scores_player_scores_PlayerScoreId",
                        column: x => x.PlayerScoreId,
                        principalTable: "player_scores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stroke_specs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    outcome = table.Column<int>(type: "integer", nullable: false),
                    put_distance = table.Column<int>(type: "integer", nullable: true),
                    HoleScoreId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stroke_specs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stroke_specs_hole_scores_HoleScoreId",
                        column: x => x.HoleScoreId,
                        principalTable: "hole_scores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_holes_CourseId",
                table: "course_holes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_global_feed_items_registered_at",
                table: "global_feed_items",
                column: "registered_at");

            migrationBuilder.CreateIndex(
                name: "IX_hole_scores_PlayerScoreId",
                table: "hole_scores",
                column: "PlayerScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_player_scores_RoundId",
                table: "player_scores",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_player_signatures_RoundId",
                table: "player_signatures",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_rating_changes_RoundId",
                table: "rating_changes",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_reset_password_requests_email",
                table: "reset_password_requests",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_stroke_specs_HoleScoreId",
                table: "stroke_specs",
                column: "HoleScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_user_feed_items_registered_at",
                table: "user_feed_items",
                column: "registered_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_feed_items_username",
                table: "user_feed_items",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_user_ratings_UserId",
                table: "user_ratings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_holes");

            migrationBuilder.DropTable(
                name: "global_feed_items");

            migrationBuilder.DropTable(
                name: "hall_of_fames");

            migrationBuilder.DropTable(
                name: "player_course_stats");

            migrationBuilder.DropTable(
                name: "player_signatures");

            migrationBuilder.DropTable(
                name: "rating_changes");

            migrationBuilder.DropTable(
                name: "reset_password_requests");

            migrationBuilder.DropTable(
                name: "stroke_specs");

            migrationBuilder.DropTable(
                name: "tournaments");

            migrationBuilder.DropTable(
                name: "user_feed_items");

            migrationBuilder.DropTable(
                name: "user_ratings");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "hole_scores");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "player_scores");

            migrationBuilder.DropTable(
                name: "rounds");
        }
    }
}
