using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Clean up invalid data before adding foreign key constraints
            migrationBuilder.Sql(@"
                PRINT 'Starting Chat Data Cleanup...';
                
                -- Remove ChatMessageReactions for orphaned messages (only if table exists)
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessageReactions')
                BEGIN
                    DELETE FROM ChatMessageReactions 
                    WHERE MessageId IN (
                        SELECT cm.Id 
                        FROM ChatMessages cm 
                        LEFT JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id 
                        WHERE cr.Id IS NULL
                    );
                    PRINT 'ChatMessageReactions cleaned';
                END
                
                -- Remove orphaned ChatMessages
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
                BEGIN
                    DELETE FROM ChatMessages 
                    WHERE ChatRoomId NOT IN (SELECT Id FROM ChatRooms);
                    PRINT 'Orphaned ChatMessages removed';
                END
                
                -- Remove ChatRooms with invalid User1Id references
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRooms')
                BEGIN
                    DELETE FROM ChatRooms 
                    WHERE User1Id NOT IN (SELECT Id FROM AspNetUsers);
                    PRINT 'ChatRooms with invalid User1Id removed';
                    
                    -- Remove ChatRooms with invalid User2Id references
                    DELETE FROM ChatRooms 
                    WHERE User2Id NOT IN (SELECT Id FROM AspNetUsers);
                    PRINT 'ChatRooms with invalid User2Id removed';
                    
                    -- Remove ChatRooms with invalid PropertyId references
                    DELETE FROM ChatRooms 
                    WHERE PropertyId NOT IN (SELECT Id FROM Properties);
                    PRINT 'ChatRooms with invalid PropertyId removed';
                END
                
                PRINT 'Chat data cleanup completed successfully!';
            ");

            migrationBuilder.CreateTable(
                name: "ChatMessageStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessageStatuses_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConnections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_PropertyId",
                table: "ChatRooms",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_User1Id",
                table: "ChatRooms",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_User2Id",
                table: "ChatRooms",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageStatuses_MessageId",
                table: "ChatMessageStatuses",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageStatuses_MessageId_UserId",
                table: "ChatMessageStatuses",
                columns: new[] { "MessageId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_UserId_ConnectionId",
                table: "UserConnections",
                columns: new[] { "UserId", "ConnectionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_AspNetUsers_User1Id",
                table: "ChatRooms",
                column: "User1Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_AspNetUsers_User2Id",
                table: "ChatRooms",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Properties_PropertyId",
                table: "ChatRooms",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_AspNetUsers_User1Id",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_AspNetUsers_User2Id",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Properties_PropertyId",
                table: "ChatRooms");

            migrationBuilder.DropTable(
                name: "ChatMessageStatuses");

            migrationBuilder.DropTable(
                name: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_PropertyId",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_User1Id",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_User2Id",
                table: "ChatRooms");
        }
    }
}
