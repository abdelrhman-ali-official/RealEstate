using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    PropertyLimit = table.Column<int>(type: "int", nullable: false),
                    ShowPropertyViews = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowWishlistNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowWishlistUserDetails = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FunnelTracking = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExportLeads = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DirectContactSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WhatsAppIntegration = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    BrokerId = table.Column<int>(type: "int", nullable: true),
                    DeveloperId = table.Column<int>(type: "int", nullable: true),
                    SubscribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPropertyCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.CheckConstraint("CK_Subscription_UserType", "([BrokerId] IS NOT NULL AND [DeveloperId] IS NULL) OR ([BrokerId] IS NULL AND [DeveloperId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Brokers_BrokerId",
                        column: x => x.BrokerId,
                        principalTable: "Brokers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IsActive",
                table: "Packages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Name",
                table: "Packages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_BrokerId_IsActive",
                table: "Subscriptions",
                columns: new[] { "BrokerId", "IsActive" },
                unique: true,
                filter: "[BrokerId] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DeveloperId_IsActive",
                table: "Subscriptions",
                columns: new[] { "DeveloperId", "IsActive" },
                unique: true,
                filter: "[DeveloperId] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PackageId",
                table: "Subscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscribedAt",
                table: "Subscriptions",
                column: "SubscribedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
