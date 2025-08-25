using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initialdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnglishWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TextNormalized = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnglishWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TurkishWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TextNormalized = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurkishWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnglishWordRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    RelatedEnglishWordId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnglishWordRelations", x => x.Id);
                    table.CheckConstraint("CK_EnglishWordRelations_Order", "[EnglishWordId] < [RelatedEnglishWordId]");
                    table.ForeignKey(
                        name: "FK_EnglishWordRelations_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnglishWordRelations_EnglishWords_RelatedEnglishWordId",
                        column: x => x.RelatedEnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExampleSentences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    EnglishText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleSentences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExampleSentences_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    TurkishWordId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordTranslations_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordTranslations_TurkishWords_TurkishWordId",
                        column: x => x.TurkishWordId,
                        principalTable: "TurkishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordListId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CycleOrderCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CycleIndex = table.Column<int>(type: "int", nullable: false),
                    SeedCount = table.Column<int>(type: "int", nullable: false),
                    SeedWordIdsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizRuns_WordLists_WordListId",
                        column: x => x.WordListId,
                        principalTable: "WordLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WordListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    WordListId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordListItems_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordListItems_WordLists_WordListId",
                        column: x => x.WordListId,
                        principalTable: "WordLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordListId = table.Column<int>(type: "int", nullable: false),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    TimesShown = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CorrectCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WrongCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastShownAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordStats_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordStats_WordLists_WordListId",
                        column: x => x.WordListId,
                        principalTable: "WordLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizRunId = table.Column<int>(type: "int", nullable: false),
                    EnglishWordId = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_EnglishWords_EnglishWordId",
                        column: x => x.EnglishWordId,
                        principalTable: "EnglishWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_QuizRuns_QuizRunId",
                        column: x => x.QuizRunId,
                        principalTable: "QuizRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnglishWordRelations_EnglishWordId_RelatedEnglishWordId",
                table: "EnglishWordRelations",
                columns: new[] { "EnglishWordId", "RelatedEnglishWordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnglishWordRelations_RelatedEnglishWordId",
                table: "EnglishWordRelations",
                column: "RelatedEnglishWordId");

            migrationBuilder.CreateIndex(
                name: "IX_EnglishWords_TextNormalized",
                table: "EnglishWords",
                column: "TextNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExampleSentences_EnglishWordId_EnglishText",
                table: "ExampleSentences",
                columns: new[] { "EnglishWordId", "EnglishText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_EnglishWordId_CreatedAt",
                table: "QuizAttempts",
                columns: new[] { "EnglishWordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizRunId",
                table: "QuizAttempts",
                column: "QuizRunId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizRuns_WordListId_StartedAt",
                table: "QuizRuns",
                columns: new[] { "WordListId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TurkishWords_TextNormalized",
                table: "TurkishWords",
                column: "TextNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordListItems_EnglishWordId",
                table: "WordListItems",
                column: "EnglishWordId");

            migrationBuilder.CreateIndex(
                name: "IX_WordListItems_WordListId_EnglishWordId",
                table: "WordListItems",
                columns: new[] { "WordListId", "EnglishWordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordLists_Name",
                table: "WordLists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_CorrectCount",
                table: "WordStats",
                column: "CorrectCount");

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_EnglishWordId",
                table: "WordStats",
                column: "EnglishWordId");

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_LastShownAt",
                table: "WordStats",
                column: "LastShownAt");

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_TimesShown",
                table: "WordStats",
                column: "TimesShown");

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_WordListId_EnglishWordId",
                table: "WordStats",
                columns: new[] { "WordListId", "EnglishWordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordStats_WrongCount",
                table: "WordStats",
                column: "WrongCount");

            migrationBuilder.CreateIndex(
                name: "IX_WordTranslations_EnglishWordId_TurkishWordId",
                table: "WordTranslations",
                columns: new[] { "EnglishWordId", "TurkishWordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordTranslations_TurkishWordId",
                table: "WordTranslations",
                column: "TurkishWordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnglishWordRelations");

            migrationBuilder.DropTable(
                name: "ExampleSentences");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "WordListItems");

            migrationBuilder.DropTable(
                name: "WordStats");

            migrationBuilder.DropTable(
                name: "WordTranslations");

            migrationBuilder.DropTable(
                name: "QuizRuns");

            migrationBuilder.DropTable(
                name: "EnglishWords");

            migrationBuilder.DropTable(
                name: "TurkishWords");

            migrationBuilder.DropTable(
                name: "WordLists");
        }
    }
}
