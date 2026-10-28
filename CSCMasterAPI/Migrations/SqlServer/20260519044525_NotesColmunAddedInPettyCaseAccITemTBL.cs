using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSCMasterAPI.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class NotesColmunAddedInPettyCaseAccITemTBL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PettyCashAccountItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PettyCashAccountItem");
        }
    }
}
