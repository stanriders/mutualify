using Hangfire.Dashboard;

namespace Mutualify.Jobs;

public class AuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.Identity.Name == "7217455";
    }
}

