using Microsoft.Extensions.DependencyInjection;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Payment;
using SmartCareerPath.Application.ServicesImplementation.Payment;
using SmartCareerPath.Application.Strategies.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application
{
    public static class PaymentDependencyInjection
    {
        
        public static IServiceCollection AddPaymentServices(this IServiceCollection services)
        {
            // Register payment strategies
            services.AddTransient<IPaymentStrategy, StripePaymentStrategy>();
            services.AddTransient<IPaymentStrategy, PayPalPaymentStrategy>();
            services.AddTransient<IPaymentStrategy, PaymobPaymentStrategy>();

            // Register strategy factory
            services.AddScoped<PaymentStrategyFactory>();

            // Register payment service
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }
    }
}
