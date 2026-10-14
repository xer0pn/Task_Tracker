using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Work" },
                    { 2, "Personal" },
                    { 3, "Learning" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "DueDate", "Priority", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Write the first version of the product brief.", null, "High", "Done", "Prepare project brief", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "High", "InProgress", "Review pull request", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Medium", "Todo", "Plan next sprint", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Medium", "Todo", "Book dentist appointment", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 2, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Low", "Done", "Buy groceries", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 2, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Low", "Todo", "Organize home office", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Medium", "InProgress", "Read EF Core documentation", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 3, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Medium", "Done", "Practice Vue forms", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 3, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "High", "Todo", "Study REST conventions", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 3, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, null, "Low", "Todo", "Write technical notes", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CategoryId",
                table: "Tasks",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
