using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationManagement.Migrations
{
    /// <inheritdoc />
    public partial class validationsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_AspNetUsers_UserId1",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId1",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Drivers",
                newName: "userId");

            migrationBuilder.AddColumn<int>(
                name: "driverId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "driverId",
                table: "FuelEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "licenseNumber",
                table: "Drivers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "contactNumber",
                table: "Drivers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "userId",
                table: "Drivers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "driverId1",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_driverId",
                table: "MaintenanceRecords",
                column: "driverId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_driverId",
                table: "FuelEntries",
                column: "driverId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_driverId1",
                table: "Drivers",
                column: "driverId1");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_userId",
                table: "Drivers",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_AspNetUsers_userId",
                table: "Drivers",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Drivers_driverId1",
                table: "Drivers",
                column: "driverId1",
                principalTable: "Drivers",
                principalColumn: "driverId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelEntries_Drivers_driverId",
                table: "FuelEntries",
                column: "driverId",
                principalTable: "Drivers",
                principalColumn: "driverId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Drivers_driverId",
                table: "MaintenanceRecords",
                column: "driverId",
                principalTable: "Drivers",
                principalColumn: "driverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_AspNetUsers_userId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Drivers_driverId1",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_FuelEntries_Drivers_driverId",
                table: "FuelEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Drivers_driverId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_driverId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_FuelEntries_driverId",
                table: "FuelEntries");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_driverId1",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_userId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "driverId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "driverId",
                table: "FuelEntries");

            migrationBuilder.DropColumn(
                name: "driverId1",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Drivers",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "licenseNumber",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "contactNumber",
                table: "Drivers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Drivers",
                type: "nvarchar(450)",
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
    }
}
