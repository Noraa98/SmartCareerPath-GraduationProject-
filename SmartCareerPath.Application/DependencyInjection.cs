using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartCareerPath.Application.PipelineBehaviors;
using System.Reflection;
using FluentValidation;
using AutoMapper;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Email;
using SmartCareerPath.Application.ServicesImplementation.Email;


namespace SmartCareerPath.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR - Register all handlers from this assembly
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // FluentValidation - Register all validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            // AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Email Service
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
