using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence;
using CSCMasterAPI.Persistence.Service;
using CSCMasterAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
//using Serilog;

//// Configure Serilog as the logging provider
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File(
//        path: "logs/log.txt",
//        rollingInterval: Serilog.RollingInterval.Day
//    ).CreateLogger();

// PostgreSQL: date columns stay "timestamp without time zone" (like SQL Server datetime2); the app decides
// which time zone a column holds (Utils/AppClock.cs). DateTime.MinValue/MaxValue stored as 0001-01-01/9999-12-31.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);
// Machine-specific settings (e.g. database password); this file is not committed.
// Environment variables and command-line arguments are re-added so they still win over it.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

// Business time zone for "today" and business dates, independent of the server's time zone.
AppClock.Configure(builder.Configuration["BusinessTimeZone"]);

// Use Serilog for logging
//builder.Host.UseSerilog(Log.Logger);

builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseConfiguredDatabase(builder.Configuration)   // SQL Server or PostgreSQL ("DatabaseProvider")
);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEnrolmentService, EnrolmentService>();
builder.Services.AddScoped<IPettyCashService, PettyCashService>();
builder.Services.AddScoped<ILoginContext, LoginContext>();

// Browser addresses allowed to call the API: the Cors:AllowedOrigins list, plus Cors:Origins
// (comma-separated, handy in an environment variable, e.g. CORS_ORIGINS in the Docker .env).
var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    .Concat((builder.Configuration["Cors:Origins"] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .Select(o => o.TrimEnd('/'))
    .Where(o => o.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
        // .AllowCredentials(); // only if you're using cookies or auth headers
    });
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new BusinessDateTimeJsonConverter()));

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<NotPaidRedirectFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReXLPos", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token.
                        Example: Bearer 12345abcdef",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var key = JwtSettings.GetKey(builder.Configuration, builder.Environment); // "Jwt:Key" / Jwt__Key
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Behind the server's main Caddy (reverse proxy): use the original scheme and client IP.
// No effect when there are no X-Forwarded-* headers.
var forwardedHeaders = new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost
};
forwardedHeaders.KnownIPNetworks.Clear();   // the app is only reachable through the proxy
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors();

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<LoginContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
