// Copies all data from the CSCMaster SQL Server database into the PostgreSQL database
// created by the EF migrations (update.sh / "dotnet ef database update" first).
//
// Usage:
//   dotnet run --project tools/CSCMasterDataCopy -- "<sql server connection>" "<postgres connection>"
//
// - Copies every table that exists in both databases, using the columns they share.
// - Empties the PostgreSQL tables first, so it can be re-run safely.
// - Foreign-key checks are switched off during the copy (needs a PostgreSQL superuser).
// - Moves the identity (id) sequences past the copied ids, so new rows get new ids.
// - Converts system timestamps to UTC if the SQL Server source is still on India time.
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// DateTime.MinValue/MaxValue as 0001-01-01 / 9999-12-31 (as in SQL Server), not -infinity/infinity.
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

if (args.Length != 2)
{
    Console.WriteLine("Usage: CSCMasterDataCopy \"<sql server connection>\" \"<postgres connection>\"");
    return 1;
}

await using var sql = new SqlConnection(args[0]);
await using var pg = new NpgsqlConnection(args[1]);
await sql.OpenAsync();
await pg.OpenAsync();

var pgTables = await PgTablesAsync(pg);
var sqlTables = await SqlTablesAsync(sql);

var skipped = sqlTables.Keys.Where(t => !pgTables.ContainsKey(t) && t != "__EFMigrationsHistory").ToList();
if (skipped.Count > 0)
    Console.WriteLine($"Not in PostgreSQL model, skipped: {string.Join(", ", skipped)}");

await ExecAsync(pg, "SET session_replication_role = replica;");   // disables triggers + FK checks for this session
await ExecAsync(pg, "TRUNCATE " + string.Join(", ", pgTables.Keys.Where(t => t != "__EFMigrationsHistory").Select(Quote)) + ";");

var failures = 0;
var total = Stopwatch.StartNew();
foreach (var (table, pgCols) in pgTables.OrderBy(t => t.Key))
{
    if (table == "__EFMigrationsHistory") continue;
    if (!sqlTables.TryGetValue(table, out var sqlCols))
    {
        Console.WriteLine($"{table,-40} not in SQL Server, left empty");
        continue;
    }

    var common = pgCols.Where(c => sqlCols.Contains(c.Name)).ToList();
    foreach (var missing in pgCols.Where(c => !sqlCols.Contains(c.Name)))
        Console.WriteLine($"{table,-40} column {missing.Name} not in SQL Server, using {(missing.HasDefault || missing.Nullable ? "database default" : "type default")}");
    // NOT NULL columns without a default that SQL Server doesn't have must still get a value.
    var filled = pgCols.Where(c => !sqlCols.Contains(c.Name) && !c.Nullable && !c.HasDefault).ToList();

    var sw = Stopwatch.StartNew();
    long rows = 0;
    try
    {
        var targetCols = common.Concat(filled).ToList();
        var copySql = $"COPY {Quote(table)} ({string.Join(", ", targetCols.Select(c => Quote(c.Name)))}) FROM STDIN (FORMAT BINARY)";
        await using var cmd = new SqlCommand($"SELECT {string.Join(", ", common.Select(c => "[" + c.Name + "]"))} FROM [{table}]", sql) { CommandTimeout = 0 };
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
        await using (var importer = await pg.BeginBinaryImportAsync(copySql))
        {
            while (await reader.ReadAsync())
            {
                await importer.StartRowAsync();
                for (var i = 0; i < common.Count; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value == null) await importer.WriteNullAsync();
                    else await importer.WriteAsync(Convert(value, common[i]), common[i].DbType);
                }
                foreach (var c in filled)
                    await importer.WriteAsync(TypeDefault(c), c.DbType);
                rows++;
            }
            await importer.CompleteAsync();
        }
        Console.WriteLine($"{table,-40} {rows,10:N0} rows  {sw.Elapsed.TotalSeconds,6:N1}s");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"{table,-40} FAILED after {rows:N0} rows: {ex.Message}");
    }
}

await ConvertTimestampsIfNeededAsync(sql, pg);
await ResetIdentitySequencesAsync(pg);
await ExecAsync(pg, "SET session_replication_role = DEFAULT;");
await VerifyCountsAsync(sql, pg, pgTables.Keys.Where(t => sqlTables.ContainsKey(t) && t != "__EFMigrationsHistory"));

Console.WriteLine(failures == 0 ? $"Done in {total.Elapsed.TotalMinutes:N1} min." : $"Finished with {failures} failed table(s).");
return failures == 0 ? 0 : 2;

// ---------------------------------------------------------------------------

// System timestamps are stored in UTC (CSCMasterAPI/Utils/AppClock.cs). The SQL Server database gets them in
// UTC with its migration UseUtcForTimestamps; before that they are India time. PostgreSQL always holds UTC,
// so convert the copied values if the source has not had that migration. Keep in sync with the migration.
static async Task ConvertTimestampsIfNeededAsync(SqlConnection sql, NpgsqlConnection pg)
{
    await using var sc = new SqlCommand("SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId LIKE '%[_]UseUtcForTimestamps'", sql);
    var sourceIsUtc = (int)(await sc.ExecuteScalarAsync())! > 0;
    if (sourceIsUtc)
    {
        Console.WriteLine("Timestamps: source already UTC - copied as-is");
        return;
    }
    (string Table, string Column)[] columns = { ("Entrollment", "UploadedDate") };
    foreach (var (table, column) in columns)
    {
        await ExecAsync(pg, $"UPDATE {Quote(table)} SET {Quote(column)} = ({Quote(column)} AT TIME ZONE 'Asia/Kolkata') AT TIME ZONE 'UTC' " +
                            $"WHERE {Quote(column)} > '0001-01-03' AND {Quote(column)} < '9999-12-29'");
    }
    Console.WriteLine("Timestamps: source is India time, converted to UTC");
}

// Copied rows keep their ids; move every identity sequence past the highest id (same as migration ResetIdentitySequences).
static async Task ResetIdentitySequencesAsync(NpgsqlConnection pg)
{
    await ExecAsync(pg, @"
DO $$
DECLARE r record; max_id bigint;
BEGIN
    FOR r IN SELECT table_name, column_name FROM information_schema.columns WHERE table_schema = 'public' AND is_identity = 'YES'
    LOOP
        EXECUTE format('SELECT max(%I) FROM %I', r.column_name, r.table_name) INTO max_id;
        PERFORM setval(pg_get_serial_sequence(format('%I', r.table_name), r.column_name), COALESCE(max_id, 0) + 1, false);
    END LOOP;
END
$$;");
    Console.WriteLine("Identity sequences moved past the copied ids");
}

static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";

static async Task ExecAsync(NpgsqlConnection pg, string text)
{
    await using var cmd = new NpgsqlCommand(text, pg) { CommandTimeout = 0 };
    await cmd.ExecuteNonQueryAsync();
}

static async Task<Dictionary<string, List<PgColumn>>> PgTablesAsync(NpgsqlConnection pg)
{
    const string q = @"SELECT c.table_name, c.column_name, c.data_type, c.is_nullable = 'YES', c.column_default IS NOT NULL
                       FROM information_schema.columns c
                       JOIN information_schema.tables t ON t.table_schema = c.table_schema AND t.table_name = c.table_name
                       WHERE c.table_schema = 'public' AND t.table_type = 'BASE TABLE'
                       ORDER BY c.table_name, c.ordinal_position";
    var result = new Dictionary<string, List<PgColumn>>();
    await using var cmd = new NpgsqlCommand(q, pg);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
    {
        var table = r.GetString(0);
        if (!result.TryGetValue(table, out var cols)) result[table] = cols = new List<PgColumn>();
        cols.Add(new PgColumn(r.GetString(1), r.GetString(2), r.GetBoolean(3), r.GetBoolean(4)));
    }
    return result;
}

static async Task<Dictionary<string, HashSet<string>>> SqlTablesAsync(SqlConnection sql)
{
    const string q = @"SELECT c.TABLE_NAME, c.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS c
                       JOIN INFORMATION_SCHEMA.TABLES t ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
                       WHERE t.TABLE_TYPE = 'BASE TABLE' AND c.TABLE_SCHEMA = 'dbo'";
    var result = new Dictionary<string, HashSet<string>>();
    await using var cmd = new SqlCommand(q, sql);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
    {
        var table = r.GetString(0);
        if (!result.TryGetValue(table, out var cols)) result[table] = cols = new HashSet<string>();
        cols.Add(r.GetString(1));
    }
    return result;
}

static object Convert(object value, PgColumn col) => col.DataType switch
{
    "integer" => System.Convert.ToInt32(value),
    "smallint" => System.Convert.ToInt16(value),
    "bigint" => System.Convert.ToInt64(value),
    "boolean" => System.Convert.ToBoolean(value),
    "double precision" => System.Convert.ToDouble(value),
    "real" => System.Convert.ToSingle(value),
    "numeric" => System.Convert.ToDecimal(value),
    "uuid" => value is Guid g ? g : Guid.Parse(value.ToString()!),
    "timestamp without time zone" => value is DateTimeOffset dto ? dto.DateTime : DateTime.SpecifyKind(System.Convert.ToDateTime(value), DateTimeKind.Unspecified),
    "date" => System.Convert.ToDateTime(value).Date,
    "bytea" => (byte[])value,
    // PostgreSQL text cannot contain NUL characters.
    _ => value.ToString()!.Replace("\0", "")
};

static object TypeDefault(PgColumn col) => col.DataType switch
{
    "integer" => 0,
    "smallint" => (short)0,
    "bigint" => 0L,
    "boolean" => false,
    "double precision" => 0d,
    "real" => 0f,
    "numeric" => 0m,
    "uuid" => Guid.Empty,
    "timestamp without time zone" => DateTime.MinValue,
    _ => ""
};

static async Task VerifyCountsAsync(SqlConnection sql, NpgsqlConnection pg, IEnumerable<string> tables)
{
    var mismatches = 0;
    foreach (var t in tables)
    {
        await using var sc = new SqlCommand($"SELECT COUNT_BIG(*) FROM [{t}]", sql) { CommandTimeout = 0 };
        await using var pc = new NpgsqlCommand($"SELECT count(*) FROM {Quote(t)}", pg) { CommandTimeout = 0 };
        var a = (long)(await sc.ExecuteScalarAsync())!;
        var b = (long)(await pc.ExecuteScalarAsync())!;
        if (a != b) { mismatches++; Console.WriteLine($"COUNT MISMATCH {t}: SQL Server {a:N0}, PostgreSQL {b:N0}"); }
    }
    Console.WriteLine(mismatches == 0 ? "Row counts match for all tables." : $"{mismatches} table(s) with different row counts.");
}

record PgColumn(string Name, string DataType, bool Nullable, bool HasDefault)
{
    public NpgsqlDbType DbType => DataType switch
    {
        "integer" => NpgsqlDbType.Integer,
        "smallint" => NpgsqlDbType.Smallint,
        "bigint" => NpgsqlDbType.Bigint,
        "boolean" => NpgsqlDbType.Boolean,
        "double precision" => NpgsqlDbType.Double,
        "real" => NpgsqlDbType.Real,
        "numeric" => NpgsqlDbType.Numeric,
        "uuid" => NpgsqlDbType.Uuid,
        "timestamp without time zone" => NpgsqlDbType.Timestamp,
        "date" => NpgsqlDbType.Date,
        "bytea" => NpgsqlDbType.Bytea,
        "character varying" => NpgsqlDbType.Varchar,
        _ => NpgsqlDbType.Text
    };
}
