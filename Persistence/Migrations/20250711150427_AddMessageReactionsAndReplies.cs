using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageReactionsAndReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepliedToMessageId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatMessageReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessageReactions_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RepliedToMessageId",
                table: "ChatMessages",
                column: "RepliedToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReactions_MessageId_UserId",
                table: "ChatMessageReactions",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatMessages_RepliedToMessageId",
                table: "ChatMessages",
                column: "RepliedToMessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatMessages_RepliedToMessageId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatMessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_RepliedToMessageId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "RepliedToMessageId",
                table: "ChatMessages");
        }
    }
}
