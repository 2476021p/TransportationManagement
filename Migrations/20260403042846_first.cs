using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationManagement.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Drivers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "driverId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId1",
                table: "Drivers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_AspNetUsers_UserId1",
                table: "Drivers",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_AspNetUsers_UserId1",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId1",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "driverId",
                table: "AspNetUsers");
        }
    }
}
