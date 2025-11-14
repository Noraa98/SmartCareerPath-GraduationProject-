using Microsoft.AspNetCore.Authorization;

namespace SmartCareerPath.APIs.Middleware
{
    public class UserOnlyAttribute : AuthorizeAttribute
    {
        public UserOnlyAttribute()
        {
            Roles = "User";
        }
    }
}
