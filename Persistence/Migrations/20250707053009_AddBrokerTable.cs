using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Developers_DeveloperId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "DeveloperId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BrokerId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brokers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Government = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brokers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brokers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_BrokerId",
                table: "Properties",
                column: "BrokerId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Property_Owner",
                table: "Properties",
                sql: "(DeveloperId IS NOT NULL AND BrokerId IS NULL) OR (DeveloperId IS NULL AND BrokerId IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_UserId",
                table: "Brokers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Brokers_BrokerId",
                table: "Properties",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Developers_DeveloperId",
                table: "Properties",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Brokers_BrokerId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Developers_DeveloperId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "Brokers");

            migrationBuilder.DropIndex(
                name: "IX_Properties_BrokerId",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Property_Owner",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BrokerId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "DeveloperId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Developers_DeveloperId",
                table: "Properties",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
