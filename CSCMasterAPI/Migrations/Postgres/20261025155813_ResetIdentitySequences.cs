using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSCMasterAPI.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ResetIdentitySequences : Migration
    {
        /// <summary>
        /// Seed rows (HasData) are inserted with fixed ids, which does not advance PostgreSQL identity sequences.
        /// Move every identity sequence past the highest id, so new rows do not collide with seeded ones.
        /// (tools/CSCMasterDataCopy does the same after copying data.)
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ResetSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        internal const string ResetSql = @"
DO $$
DECLARE
    r record;
    max_id bigint;
BEGIN
    FOR r IN
        SELECT table_name, column_name FROM information_schema.columns
         WHERE table_schema = 'public' AND is_identity = 'YES'
    LOOP
        EXECUTE format('SELECT max(%I) FROM %I', r.column_name, r.table_name) INTO max_id;
        PERFORM setval(pg_get_serial_sequence(format('%I', r.table_name), r.column_name), COALESCE(max_id, 0) + 1, false);
    END LOOP;
END
$$;";
    }
}
