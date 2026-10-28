# Database: SQL Server or PostgreSQL

The API runs on either database. Pick one in `CSCMasterAPI/appsettings.json` (or `appsettings.Local.json`, which is
not committed, or an environment variable):

```json
"DatabaseProvider": "Postgres",          // or "SqlServer" (the default)
"ConnectionStrings": {
  "SqlServer": "Server=...;Database=CSCMaster;...",
  "Postgres":  "Host=...;Port=5432;Database=cscmaster;Username=...;Password=..."
}
```

Environment variable form (used in Docker): `DatabaseProvider=Postgres` and `ConnectionStrings__Postgres=...`.

PostgreSQL must be **version 18 or later** (case-insensitive `LIKE` searches need it). On PostgreSQL all text
columns use a case-insensitive collation, so searches and logins behave as on SQL Server.

## Where things are

| Path | Contents |
|---|---|
| `CSCMasterAPI/Persistence/ApplicationDbContext.cs` | Entities, seed data, provider-specific settings (`IsPostgres`) |
| `CSCMasterAPI/Persistence/DatabaseSetup.cs` | Chooses the provider; makes EF use only that provider's migrations |
| `CSCMasterAPI/Migrations/SqlServer` | SQL Server migrations (the original ones) |
| `CSCMasterAPI/Migrations/Postgres` | PostgreSQL migrations |
| `tools/CSCMasterDataCopy` | Copies all data from SQL Server into PostgreSQL |

Both migration sets are in the API project; `DatabaseSetup.cs` picks the set by namespace
(`CSCMasterAPI.Migrations.SqlServer` / `CSCMasterAPI.Migrations.Postgres`).

## Changing the database schema

Every model change needs a migration **for both databases**. Use the local EF tool (`dotnet tool restore` once):

```bash
DatabaseProvider=SqlServer dotnet tool run dotnet-ef migrations add <Name> --project CSCMasterAPI --output-dir Migrations/SqlServer --namespace CSCMasterAPI.Migrations.SqlServer
DatabaseProvider=Postgres  dotnet tool run dotnet-ef migrations add <Name> --project CSCMasterAPI --output-dir Migrations/Postgres  --namespace CSCMasterAPI.Migrations.Postgres
```

(On Windows PowerShell set the variable first: `$env:DatabaseProvider="Postgres"`.)
Raw SQL in a migration must be written in that database's dialect.

If a migration adds seed rows (`HasData`) to a PostgreSQL table, add a step that moves the id counter past them
(see migration `ResetIdentitySequences`), or the next insert fails with a duplicate key.

## Setting up a new PostgreSQL database from SQL Server

```bash
DatabaseProvider=Postgres dotnet tool run dotnet-ef database update --project CSCMasterAPI
dotnet run --project tools/CSCMasterDataCopy -- "<sql server connection>" "<postgres connection>"
```

The copy tool empties the PostgreSQL tables first, copies every table, moves the id counters past the copied ids,
and checks the row counts. Run it with a PostgreSQL superuser (it switches foreign-key checks off during the copy).
If the SQL Server source has not had the `UseUtcForTimestamps` migration yet, the tool converts the enrolment
upload times from India time to UTC while copying. On the server, `import-sqlserver.sh` does all of this from a
`.bak` file (see DOCKER.md).

## Dates

The app works the same whatever the server's time zone. Two kinds of date values:

| Kind | Columns | Stored as | In code |
|---|---|---|---|
| **Business dates** | `PettyCashAccount.Date`, `Entrollment.EIDDate`, report From/To dates | Business time zone (`BusinessTimeZone`, default `Asia/Kolkata`) | `AppClock.Now`, `AppClock.Today` |
| **System timestamps** | `Entrollment.UploadedDate` | UTC | `DateTime.UtcNow`; show with `AppClock.ToBusinessTime(...)` |

Rules:
- Never use `DateTime.Now` / `DateTime.Today` (they depend on the server). Use `AppClock`
  (`CSCMasterAPI/Utils/AppClock.cs`).
- Dates in API requests are converted to business time by `BusinessDateTimeJsonConverter`: the UI date pickers
  send local dates, and a UTC date (ending in `Z`, from JavaScript `toISOString()`) becomes the same business date.
- A new system-timestamp column needs `.HasConversion(AppClock.UtcConverter)` in `ApplicationDbContext`, so it is
  read back as UTC.
- On SQL Server, migration `UseUtcForTimestamps` converts the existing `UploadedDate` values from India time to UTC.

## Settings that are secrets

`Jwt:Key` (environment variable `Jwt__Key`; `JWT_KEY` in the Docker `.env`) signs the login tokens. It must be at
least 32 characters and is required outside Development. Keep it out of `appsettings.json`: put it in
`appsettings.Local.json` or an environment variable.
