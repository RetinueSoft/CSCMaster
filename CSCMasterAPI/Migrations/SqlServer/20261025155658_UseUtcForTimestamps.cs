using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSCMasterAPI.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UseUtcForTimestamps : Migration
    {
        /// <summary>
        /// Entrollment.UploadedDate (time of the import) moves to UTC; existing values were recorded on a server
        /// running on India time. Business dates (EIDDate, OnboardDate, petty cash dates) are not changed.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Entrollment] SET [UploadedDate] = CAST([UploadedDate] AT TIME ZONE 'India Standard Time' AT TIME ZONE 'UTC' AS datetime2)
 WHERE [UploadedDate] > '0001-01-03' AND [UploadedDate] < '9999-12-29';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Entrollment] SET [UploadedDate] = CAST([UploadedDate] AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time' AS datetime2)
 WHERE [UploadedDate] > '0001-01-03' AND [UploadedDate] < '9999-12-29';");
        }
    }
}
