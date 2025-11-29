using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SmartCareerPath.APIs.Middleware
{
    public static class JwtAuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"] ?? "SmartCareerPath";
            var audience = jwtSettings["Audience"] ?? "SmartCareerPathUsers";

            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException("JWT Secret is not configured in appsettings.json");

            var key = Encoding.UTF8.GetBytes(secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // No tolerance for expired tokens
                    RequireExpirationTime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtBearerEvents");

                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader))
                            {
                                var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
                                logger?.LogInformation("OnMessageReceived - Authorization header present. Token length: {len}", token?.Length ?? 0);
                            }
                            else
                            {
                                logger?.LogInformation("OnMessageReceived - No Authorization header present");
                            }
                        }
                        catch { /* swallow logging errors */ }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtBearerEvents");

                            logger?.LogError(context.Exception, "JWT authentication failed: {message}", context.Exception?.Message);

                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                        }
                        catch { /* swallow logging errors */ }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Unauthorized",
                            message = "You are not authorized to access this resource"
                        });

                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Forbidden",
                            message = "You do not have permission to access this resource"
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

            return services;
        }
    }

}
