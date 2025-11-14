namespace SmartCareerPath.APIs.Middleware
{
    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string EmployerOrAdmin = "EmployerOrAdmin";
        public const string VerifiedEmailRequired = "VerifiedEmailRequired";
        public const string ActiveSubscription = "ActiveSubscription";

        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Admin only policy
                options.AddPolicy(AdminOnly, policy =>
                    policy.RequireRole("Admin"));

                // Employer or Admin policy
                options.AddPolicy(EmployerOrAdmin, policy =>
                    policy.RequireRole("Employer", "Admin"));

                // Verified email required
                options.AddPolicy(VerifiedEmailRequired, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "EmailVerified" && c.Value == "True")));

                // Active subscription required
                options.AddPolicy(ActiveSubscription, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "HasActiveSubscription" && c.Value == "True")));
            });

            return services;
        }
    }
}
