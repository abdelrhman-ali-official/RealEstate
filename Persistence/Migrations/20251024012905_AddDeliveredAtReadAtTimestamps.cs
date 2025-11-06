using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveredAtReadAtTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChatMessageId",
                table: "ChatMessageStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelivered",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChatMessageId",
                table: "ChatMessageReactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageStatuses_ChatMessageId",
                table: "ChatMessageStatuses",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReactions_ChatMessageId",
                table: "ChatMessageReactions",
                column: "ChatMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessageReactions_ChatMessages_ChatMessageId",
                table: "ChatMessageReactions",
                column: "ChatMessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessageStatuses_ChatMessages_ChatMessageId",
                table: "ChatMessageStatuses",
                column: "ChatMessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessageReactions_ChatMessages_ChatMessageId",
                table: "ChatMessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessageStatuses_ChatMessages_ChatMessageId",
                table: "ChatMessageStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageStatuses_ChatMessageId",
                table: "ChatMessageStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageReactions_ChatMessageId",
                table: "ChatMessageReactions");

            migrationBuilder.DropColumn(
                name: "ChatMessageId",
                table: "ChatMessageStatuses");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ChatMessageId",
                table: "ChatMessageReactions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelivered",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
