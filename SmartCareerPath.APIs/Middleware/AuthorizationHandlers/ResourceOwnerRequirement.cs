using Microsoft.AspNetCore.Authorization;

namespace SmartCareerPath.APIs.Middleware.AuthorizationHandlers
{
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        public string ResourceIdParameter { get; set; }

        public ResourceOwnerRequirement(string resourceIdParameter)
        {
            ResourceIdParameter = resourceIdParameter;
        }
    }
}
