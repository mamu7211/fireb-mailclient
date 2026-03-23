using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feirb.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMailStorageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeColor",
                table: "Mailboxes",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialSyncDays",
                table: "Mailboxes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PollIntervalMinutes",
                table: "Mailboxes",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.CreateTable(
                name: "CachedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MailboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ImapUid = table.Column<long>(type: "bigint", nullable: true),
                    Subject = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    From = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReplyTo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    To = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Cc = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BodyPlainText = table.Column<string>(type: "text", nullable: true),
                    BodyHtml = table.Column<string>(type: "text", nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CachedMessages_Mailboxes_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CachedAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CachedMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Filename = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CachedAttachments_CachedMessages_CachedMessageId",
                        column: x => x.CachedMessageId,
                        principalTable: "CachedMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedAttachments_CachedMessageId",
                table: "CachedAttachments",
                column: "CachedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CachedMessages_Date",
                table: "CachedMessages",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CachedMessages_MailboxId_MessageId",
                table: "CachedMessages",
                columns: new[] { "MailboxId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedAttachments");

            migrationBuilder.DropTable(
                name: "CachedMessages");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BadgeColor",
                table: "Mailboxes");

            migrationBuilder.DropColumn(
                name: "InitialSyncDays",
                table: "Mailboxes");

            migrationBuilder.DropColumn(
                name: "PollIntervalMinutes",
                table: "Mailboxes");
        }
    }
}
