using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace CSCMasterAPI.Persistence
{
    /// <summary>
    /// Chooses SQL Server or PostgreSQL from configuration.
    ///
    ///   "DatabaseProvider": "SqlServer" | "Postgres"   (default: SqlServer)
    ///   "ConnectionStrings": {
    ///       "SqlServer": "...",          // used when DatabaseProvider = SqlServer
    ///       "Postgres":  "...",          // used when DatabaseProvider = Postgres
    ///       "DefaultConnection": "..."   // fallback if the provider-specific one is missing
    ///   }
    /// Migrations for each provider live in Migrations/SqlServer and Migrations/Postgres (see ProviderMigrationsAssembly).
    /// </summary>
    public static class DatabaseSetup
    {
        public const string SqlServer = "SqlServer";
        public const string Postgres = "Postgres";

        public static string GetProvider(IConfiguration config)
        {
            var provider = config["DatabaseProvider"];
            if (string.IsNullOrWhiteSpace(provider) || provider.Equals(SqlServer, StringComparison.OrdinalIgnoreCase))
                return SqlServer;
            if (provider.Equals(Postgres, StringComparison.OrdinalIgnoreCase) || provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                return Postgres;
            throw new InvalidOperationException($"Unknown DatabaseProvider '{provider}'. Use '{SqlServer}' or '{Postgres}'.");
        }

        public static string GetConnectionString(IConfiguration config)
        {
            var provider = GetProvider(config);
            var conn = config.GetConnectionString(provider) ?? config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException($"No connection string configured for provider '{provider}'.");
            return conn;
        }

        public static DbContextOptionsBuilder UseConfiguredDatabase(this DbContextOptionsBuilder builder, IConfiguration config)
        {
            var conn = GetConnectionString(config);
            if (GetProvider(config) == Postgres)
                builder.UseNpgsql(conn);
            else
                builder.UseSqlServer(conn);
            // Both providers' migrations are in this assembly; pick the right set.
            builder.ReplaceService<IMigrationsAssembly, ProviderMigrationsAssembly>();
            return builder;
        }
    }

    /// <summary>
    /// Only exposes the migrations (and model snapshot) of the active provider:
    /// namespace CSCMasterAPI.Migrations.SqlServer or CSCMasterAPI.Migrations.Postgres.
    /// </summary>
#pragma warning disable EF1001 // MigrationsAssembly is an EF internal API; this is the documented way to filter migrations.
    public sealed class ProviderMigrationsAssembly : MigrationsAssembly
    {
        private readonly string _namespace;
        private readonly Type _contextType;
        private IReadOnlyDictionary<string, TypeInfo>? _migrations;
        private ModelSnapshot? _snapshot;

        public ProviderMigrationsAssembly(ICurrentDbContext currentContext, IDbContextOptions options,
            IMigrationsIdGenerator idGenerator, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
            : base(currentContext, options, idGenerator, logger)
        {
            _contextType = currentContext.Context.GetType();
            _namespace = "CSCMasterAPI.Migrations." + (currentContext.Context.Database.IsNpgsql() ? "Postgres" : "SqlServer");
        }

        public override IReadOnlyDictionary<string, TypeInfo> Migrations =>
            _migrations ??= new SortedDictionary<string, TypeInfo>(
                base.Migrations.Where(m => m.Value.Namespace == _namespace).ToDictionary(m => m.Key, m => m.Value));

        public override ModelSnapshot? ModelSnapshot =>
            _snapshot ??= Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ModelSnapshot)) && t.Namespace == _namespace &&
                            t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType)
                .Select(t => (ModelSnapshot)Activator.CreateInstance(t)!)
                .FirstOrDefault();
    }
#pragma warning restore EF1001
}
