using Microsoft.AspNetCore.Authorization;

namespace SmartCareerPath.APIs.Middleware.AuthorizationHandlers
{
    public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceOwnerHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Get resource ID from route
            var resourceId = httpContext?.Request.RouteValues[requirement.ResourceIdParameter]?.ToString();

            // Add your logic to check if user owns the resource
            // Example: Check if userId matches resource owner

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
