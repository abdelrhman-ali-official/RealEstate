using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionSystemWithPlanTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanType",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Monthly");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "Packages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "YearlyPrice",
                table: "Packages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 10000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "YearlyPrice",
                table: "Packages");
        }
    }
}
