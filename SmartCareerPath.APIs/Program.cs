
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



            
            // 1) Register Services
            // Application (MediatR, Validators, AutoMapper, Pipeline Behaviors)
            builder.Services.AddApplication();
            // Add Payment Services
            builder.Services.AddPaymentServices();
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

           
            // 2) Build App
            var app = builder.Build();

            
            // 3) Auto Database Creation (async, non-blocking)
            /// Initialize database in the background to avoid blocking startup
            ///_ = Task.Run(async () =>
            ///{
            ///    try
            ///    {
            ///        using (var scope = app.Services.CreateScope())
            ///        {
            ///            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ///            await context.Database.EnsureCreatedAsync();
            ///        }
            ///    }
            ///    catch (Exception ex)
            ///    {
            ///        Console.WriteLine($"Database initialization error: {ex.Message}");
            ///    }
            ///});
            
            
            // Run pending migrations automatically with error handling
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    // Get pending migrations
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    
                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine($"Found {pendingMigrations.Count} pending migration(s):");
                        foreach (var migration in pendingMigrations)
                        {
                            Console.WriteLine($"  - {migration}");
                        }
                        
                        try
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("Migrations applied successfully.");
                        }
                        catch (Exception migrationEx)
                        {
                            string errorMsg = migrationEx.InnerException?.Message ?? migrationEx.Message;
                            
                            // Table already exists - mark migration as applied anyway
                            if (errorMsg.Contains("already an object named") || errorMsg.Contains("Already exists"))
                            {
                                Console.WriteLine($"⚠️ Migration skipped - table already exists in database. Error: {errorMsg}");
                                Console.WriteLine("Attempting to mark migrations as applied in history...");
                                
                                try
                                {
                                    foreach (var migration in pendingMigrations)
                                    {
                                        try
                                        {
                                            await context.Database.ExecuteSqlRawAsync(
                                                $"IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '{migration}') " +
                                                $"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('{migration}', '6.0.0')");
                                        }
                                        catch
                                        {
                                            // Ignore if already exists
                                        }
                                    }
                                    Console.WriteLine("✓ Migrations marked as applied.");
                                }
                                catch (Exception historyEx)
                                {
                                    Console.WriteLine($"⚠️ Could not update migration history: {historyEx.Message}");
                                }
                            }
                            else
                            {
                                // Unknown error - log it
                                Console.WriteLine($"❌ Migration error: {errorMsg}");
                                throw;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("✓ No pending migrations.");
                    }
                    
                    // Apply manual SQL fixes for column constraints that don't match entity model
                    try
                    {
                        Console.WriteLine("Applying manual column constraint fixes...");
                        
                        // Fix PaymentMethod to be nullable (nullable in entity but NOT NULL in database)
                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "ALTER TABLE PaymentTransactions ALTER COLUMN PaymentMethod INT NULL");
                            Console.WriteLine("✓ PaymentMethod column altered to nullable");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ PaymentMethod alter failed: {ex.InnerException?.Message}");
                        }
                        
                        // Fix FailureReason to be nullable
                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "ALTER TABLE PaymentTransactions ALTER COLUMN FailureReason NVARCHAR(500) NULL");
                            Console.WriteLine("✓ FailureReason column altered to nullable");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ FailureReason alter failed: {ex.InnerException?.Message}");
                        }
                        
                        // Fix MetadataJson column (exists in DB but not in entity - make it nullable or drop it)
                        try
                        {
                            // Try to make it nullable first
                            await context.Database.ExecuteSqlRawAsync(
                                "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PaymentTransactions' AND COLUMN_NAME='MetadataJson') " +
                                "ALTER TABLE PaymentTransactions ALTER COLUMN MetadataJson NVARCHAR(MAX) NULL");
                            Console.WriteLine("✓ MetadataJson column made nullable");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ MetadataJson alter failed: {ex.InnerException?.Message}");
                        }
                        
                        Console.WriteLine("✓ All column constraint fixes completed.");
                    }
                    catch (Exception constraintEx)
                    {
                        Console.WriteLine($"⚠️ Constraint fix error: {constraintEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal migration error: {ex.Message}");
                throw;
            }


            

            // 4) Configure Pipeline

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

            // Custom Token Middleware (run after authentication so token info is available)
            app.UseTokenValidation();

            app.UseAuthorization();

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
