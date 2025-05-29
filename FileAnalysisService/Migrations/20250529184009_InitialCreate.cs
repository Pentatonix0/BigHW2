using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalysisService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParagraphCount = table.Column<int>(type: "integer", nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: false),
                    CharCount = table.Column<int>(type: "integer", nullable: false),
                    WordCloudImagePath = table.Column<string>(type: "text", nullable: true),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OtherFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimilarityPercentage = table.Column<double>(type: "double precision", nullable: false),
                    ComparisonDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_FileId",
                table: "AnalysisResults",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResults");

            migrationBuilder.DropTable(
                name: "PlagiarismResults");
        }
    }
}
