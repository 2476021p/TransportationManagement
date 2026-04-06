using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationManagement.Migrations
{
    /// <inheritdoc />
    public partial class remoedupliates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trips_driverId",
                table: "Trips");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_vehicleNumber",
                table: "Vehicles",
                column: "vehicleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_driverId_vehicleId_origin_destination",
                table: "Trips",
                columns: new[] { "driverId", "vehicleId", "origin", "destination" },
                unique: true,
                filter: "[driverId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_contactNumber",
                table: "Drivers",
                column: "contactNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_licenseNumber",
                table: "Drivers",
                column: "licenseNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_vehicleNumber",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Trips_driverId_vehicleId_origin_destination",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_contactNumber",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_licenseNumber",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_driverId",
                table: "Trips",
                column: "driverId");
        }
    }
}
