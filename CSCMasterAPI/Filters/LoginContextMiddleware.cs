using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence.Entity;
using System.Security.Claims;
using System.Text.Json;

public class LoginContextMiddleware
{
    private readonly RequestDelegate _next;

    public LoginContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILoginContext userContextService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userData = context.User.FindFirst(ClaimTypes.UserData)?.Value;
            var user = JsonSerializer.Deserialize<User>(userData);
            userContextService.CompanyId = user.CompanyId;
            userContextService.BranchId = user.BranchId;
            userContextService.UserId = Convert.ToInt32(context.User.FindFirst("UserId")?.Value);
        }

        await _next(context);
    }
}