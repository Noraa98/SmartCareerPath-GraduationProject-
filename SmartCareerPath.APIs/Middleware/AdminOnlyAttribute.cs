using Microsoft.AspNetCore.Authorization;

namespace SmartCareerPath.APIs.Middleware
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute()
        {
            Roles = "Admin";
        }
    }
}
