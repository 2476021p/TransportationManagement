using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationManagement.Migrations
{
    /// <inheritdoc />
    public partial class secondcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Drivers_driverId",
                table: "Trips");

            migrationBuilder.AlterColumn<int>(
                name: "driverId",
                table: "Trips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Drivers_driverId",
                table: "Trips",
                column: "driverId",
                principalTable: "Drivers",
                principalColumn: "driverId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Drivers_driverId",
                table: "Trips");

            migrationBuilder.AlterColumn<int>(
                name: "driverId",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Drivers_driverId",
                table: "Trips",
                column: "driverId",
                principalTable: "Drivers",
                principalColumn: "driverId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
