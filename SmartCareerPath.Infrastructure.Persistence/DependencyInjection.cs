using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using SmartCareerPath.Domain.Contracts;
using SmartCareerPath.Domain.Contracts.Repositories;
using SmartCareerPath.Infrastructure.Persistence.Data;
using SmartCareerPath.Infrastructure.Persistence.Repositories;
using SmartCareerPath.Infrastructure.Persistence.Services.AI;

namespace SmartCareerPath.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

                // Enable sensitive data logging in development

                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();

            });

            // Repository Pattern
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // AI Services
            services.AddScoped<IAIService, AIService>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // If you want to add custom repositories, do it here
            // services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
