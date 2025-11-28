using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.DbInitializer
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            bool runMigrations = true,
            bool seedData = true)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                if (runMigrations)
                {
                    logger.LogInformation("Applying database migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");
                }

                
                if (seedData)
                {
                    logger.LogInformation("Seeding database...");
                    await SeedDataAsync(context, logger);
                    logger.LogInformation("Database seeding completed successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

       
        private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
        {
            // Seed in order of dependencies

            // 1. Users (if needed for testing)
            // await SeedUsersAsync(context, logger);

            // 2. Subscription data
            // await SeedSubscriptionsAsync(context, logger);

            // 3. Payment test data (only in development)
#if DEBUG
            // Seed.PaymentSeeder.SeedTestPayments(context);
#endif

            await context.SaveChangesAsync();
        }

        
        public static async Task<bool> CanConnectAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        
        public static async Task<int> GetPendingMigrationsCountAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.Count();
        }
    }
}
