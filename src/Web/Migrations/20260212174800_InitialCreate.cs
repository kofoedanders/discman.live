using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql.Extensions", new[] { "uuid-ossp" });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    layout = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: true),
                    par = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<decimal>(type: "numeric", nullable: false),
                    plays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "global_feed_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    feed_type = table.Column<string>(type: "text", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: true),
                    achievement_name = table.Column<string>(type: "text", nullable: true),
                    hole_number = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_feed_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hall_of_fames",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    best_score = table.Column<int>(type: "integer", nullable: false),
                    winner = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hall_of_fames", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_course_stats",
                columns: table => new
                {
                    username = table.Column<string>(type: "text", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    best_score = table.Column<int>(type: "integer", nullable: false),
                    plays = table.Column<int>(type: "integer", nullable: false),
                    average_score = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    // No primary key for keyless entity
                });

            migrationBuilder.CreateTable(
                name: "reset_password_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reset_password_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    discman_points = table.Column<int>(type: "integer", nullable: false),
                    elo = table.Column<decimal>(type: "numeric", nullable: false),
                    simple_scoring = table.Column<bool>(type: "boolean", nullable: false),
                    emoji = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    register_put_distance = table.Column<bool>(type: "boolean", nullable: false),
                    settings_initialized = table.Column<bool>(type: "boolean", nullable: false),
                    last_email_sent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    friends = table.Column<string[]>(type: "text[]", nullable: false),
                    news_ids_seen = table.Column<string[]>(type: "text[]", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "holes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    par = table.Column<int>(type: "integer", nullable: false),
                    distance = table.Column<int>(type: "integer", nullable: false),
                    handicap = table.Column<int>(type: "integer", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holes", x => x.id);
                    table.ForeignKey(
                        name: "FK_holes_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_feed_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    feed_type = table.Column<string>(type: "text", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: true),
                    achievement_name = table.Column<string>(type: "text", nullable: true),
                    hole_number = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_feed_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_feed_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_rating_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    rating_at_time = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_rating_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_rating_history_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spectators = table.Column<string[]>(type: "text[]", nullable: false),
                    score_mode = table.Column<int>(type: "integer", nullable: false),
                    round_name = table.Column<string>(type: "text", nullable: true),
                    course_name = table.Column<string>(type: "text", nullable: true),
                    course_layout = table.Column<string>(type: "text", nullable: true),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    achievements = table.Column<string>(type: "jsonb", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rounds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    base64_signature = table.Column<string>(type: "text", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    change = table.Column<decimal>(type: "numeric", nullable: false)
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
                name: "player_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_name = table.Column<string>(type: "text", nullable: false),
                    total_strokes = table.Column<int>(type: "integer", nullable: false),
                    total_relative_to_par = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_scores_rounds_round_id",
                        column: x => x.round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hole_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_score_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hole_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strokes = table.Column<int>(type: "integer", nullable: false),
                    ob_strokes = table.Column<int>(type: "integer", nullable: false),
                    putts = table.Column<int>(type: "integer", nullable: true),
                    relative_to_par = table.Column<int>(type: "integer", nullable: false),
                    fairway = table.Column<bool>(type: "boolean", nullable: true),
                    green_in_regulation = table.Column<bool>(type: "boolean", nullable: false),
                    penalty_strokes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hole_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_hole_scores_holes_hole_id",
                        column: x => x.hole_id,
                        principalTable: "holes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hole_scores_player_scores_player_score_id",
                        column: x => x.player_score_id,
                        principalTable: "player_scores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stroke_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hole_score_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stroke_number = table.Column<int>(type: "integer", nullable: false),
                    landing_area = table.Column<string>(type: "text", nullable: false),
                    distance_to_pin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stroke_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_stroke_specs_hole_scores_hole_score_id",
                        column: x => x.hole_score_id,
                        principalTable: "hole_scores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_global_feed_items_created_at",
                table: "global_feed_items",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_hole_scores_hole_id",
                table: "hole_scores",
                column: "hole_id");

            migrationBuilder.CreateIndex(
                name: "IX_hole_scores_player_score_id",
                table: "hole_scores",
                column: "player_score_id");

            migrationBuilder.CreateIndex(
                name: "IX_holes_course_id",
                table: "holes",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_scores_round_id",
                table: "player_scores",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_stroke_specs_hole_score_id",
                table: "stroke_specs",
                column: "hole_score_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_feed_items_user_id",
                table: "user_feed_items",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_rating_history_user_id",
                table: "user_rating_history",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_feed_items");

            migrationBuilder.DropTable(
                name: "hall_of_fames");

            migrationBuilder.DropTable(
                name: "player_course_stats");

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
                name: "user_rating_history");

            migrationBuilder.DropTable(
                name: "player_signatures");

            migrationBuilder.DropTable(
                name: "hole_scores");

            migrationBuilder.DropTable(
                name: "holes");

            migrationBuilder.DropTable(
                name: "player_scores");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "rounds");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
