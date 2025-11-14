using Microsoft.AspNetCore.Authorization;

namespace SmartCareerPath.APIs.Middleware
{
    public class EmployerOrAdminAttribute : AuthorizeAttribute
    {
        public EmployerOrAdminAttribute()
        {
            Roles = "Employer,Admin";
        }
    }
}
