using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportationManagement.Migrations
{
    /// <inheritdoc />
    public partial class validations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drivers_contactNumber",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_licenseNumber",
                table: "Drivers");

            migrationBuilder.AlterColumn<string>(
                name: "contactNumber",
                table: "Drivers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "contactNumber",
                table: "Drivers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

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
    }
}
