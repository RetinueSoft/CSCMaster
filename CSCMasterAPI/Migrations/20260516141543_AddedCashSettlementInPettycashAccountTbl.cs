using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSCMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedCashSettlementInPettycashAccountTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashAmount",
                table: "PettyCashAccount",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UPIAmount",
                table: "PettyCashAccount",
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
                name: "CashAmount",
                table: "PettyCashAccount");

            migrationBuilder.DropColumn(
                name: "UPIAmount",
                table: "PettyCashAccount");
        }
    }
}
