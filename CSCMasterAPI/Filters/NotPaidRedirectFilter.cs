using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

public class NotPaidRedirectFilter : IActionFilter
{
    private readonly bool _notPaidEnabled;

    public NotPaidRedirectFilter(IConfiguration configuration)
    {
        _notPaidEnabled = false;//configuration.GetValue<bool>("NotPaid") && DateTime.Now.Date.Day >= 18;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_notPaidEnabled)
        {
            var routeValues = context.RouteData.Values;

            if (!string.Equals(routeValues["controller"]?.ToString(), "Home", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(routeValues["action"]?.ToString(), "NotPaidIndex", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToRouteResult(new
                {
                    controller = "Home",
                    action = "NotPaidIndex"
                });
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action required after execution in this scenario
    }
}
