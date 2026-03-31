using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feirb.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMailboxImapRequiresAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImapUsername",
                table: "Mailboxes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<bool>(
                name: "ImapRequiresAuth",
                table: "Mailboxes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImapRequiresAuth",
                table: "Mailboxes");

            migrationBuilder.AlterColumn<string>(
                name: "ImapUsername",
                table: "Mailboxes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
