
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartCareerPath.APIs.Middleware;
using SmartCareerPath.Application;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using SmartCareerPath.Application.ServicesImplementation.Auth;
using SmartCareerPath.Infrastructure.Persistence;
using SmartCareerPath.Infrastructure.Persistence.Data;

namespace SmartCareerPath.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // If an OpenRouter API key is present in configuration (user-secrets or appsettings),
            // copy it into an environment variable so existing code can continue to use
            // Environment.GetEnvironmentVariable("OPENROUTER_API_KEY").
            try
            {
                var cfgKey = builder.Configuration["OpenRouter:ApiKey"] ?? builder.Configuration["OPENROUTER_API_KEY"];
                if (!string.IsNullOrWhiteSpace(cfgKey))
                {
                    Environment.SetEnvironmentVariable("OPENROUTER_API_KEY", cfgKey);
                    Console.WriteLine("[AI DEBUG] Loaded OPENROUTER_API_KEY from configuration into environment.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI DEBUG] Unable to load OpenRouter API key from configuration: {ex.Message}");
            }



            // ============================================================
            // 1) Register Services
            // ============================================================

            // Application (MediatR, Validators, AutoMapper, Pipeline Behaviors)
            builder.Services.AddApplication();

            // Infrastructure (DbContext, Repositories, Unit of Work)
            builder.Services.AddInfrastructure(builder.Configuration);

            // Auth Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();

            // JWT
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Authorization
            builder.Services.AddCustomAuthorization();

            builder.Services.AddHttpContextAccessor();

            // Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p =>
                {
                    p.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowAnyOrigin();
                });
            });

            // Swagger / OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Smart Career Path API",
                    Version = "v1",
                    Description = "API for Smart Career Path Platform"
                });

                // 🔐 JWT Auth header
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter: Bearer <token>",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Enable OpenAPI endpoint
            builder.Services.AddOpenApi();

            // ============================================================
            // 2) Build App
            // ============================================================

            var app = builder.Build();

            // ============================================================
            // 3) Auto Database Creation (async, non-blocking)
            // ============================================================

            // Initialize database in the background to avoid blocking startup
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await context.Database.EnsureCreatedAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database initialization error: {ex.Message}");
                }
            });

            // ============================================================
            // 4) Configure Pipeline
            // ============================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Career Path API v1");
                    c.RoutePrefix = "api-docs";
                });
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            // Custom Token Middleware
            app.UseTokenValidation();

            // Map Controllers
            app.MapControllers();

            // Health Check
            app.MapGet("/health", () =>
                Results.Ok(new
                {
                    status = "healthy",
                    time = DateTime.UtcNow
                })
            ).WithOpenApi();

            app.Run();
        }
    }
}
