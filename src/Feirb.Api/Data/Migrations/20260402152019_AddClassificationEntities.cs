using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Feirb.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Configuration",
                table: "JobSettings",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassificationQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CachedMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Error = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassificationQueueItems_CachedMessages_CachedMessageId",
                        column: x => x.CachedMessageId,
                        principalTable: "CachedMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassificationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CachedMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    ClassifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassificationResults_CachedMessages_CachedMessageId",
                        column: x => x.CachedMessageId,
                        principalTable: "CachedMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "JobSettings",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "Configuration",
                value: "{\"batchSize\":10}");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationQueueItems_CachedMessageId",
                table: "ClassificationQueueItems",
                column: "CachedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationQueueItems_Status_Ordinal",
                table: "ClassificationQueueItems",
                columns: new[] { "Status", "Ordinal" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationResults_CachedMessageId",
                table: "ClassificationResults",
                column: "CachedMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificationQueueItems");

            migrationBuilder.DropTable(
                name: "ClassificationResults");

            migrationBuilder.DropColumn(
                name: "Configuration",
                table: "JobSettings");
        }
    }
}
