
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Not Secure
});

//builder.Services.AddControllersWithViews(options =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//        .AddAuthenticationSchemes("JwtCookieScheme")
//        .RequireAuthenticatedUser()
//        .Build();

//    options.Filters.Add(new AuthorizeFilter(policy));
//});
builder.Services.Configure<RazorViewEngineOptions>(o =>
{
    o.ViewLocationFormats.Add("/Views/Selector/{0}" + RazorViewEngine.ViewExtension);
});

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401)
    {
        context.Response.Redirect("/");
    }
});
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
