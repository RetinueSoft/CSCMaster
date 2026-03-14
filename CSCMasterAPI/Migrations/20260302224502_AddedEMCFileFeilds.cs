using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSCMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedEMCFileFeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForeignResident",
                table: "Entrollment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PacketSkipped",
                table: "Entrollment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingStateDescription",
                table: "Entrollment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReasonDescription",
                table: "Entrollment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Entrollment",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForeignResident",
                table: "Entrollment");

            migrationBuilder.DropColumn(
                name: "PacketSkipped",
                table: "Entrollment");

            migrationBuilder.DropColumn(
                name: "ProcessingStateDescription",
                table: "Entrollment");

            migrationBuilder.DropColumn(
                name: "RejectReasonDescription",
                table: "Entrollment");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Entrollment");
        }
    }
}
