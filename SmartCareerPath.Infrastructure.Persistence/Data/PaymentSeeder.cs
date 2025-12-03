using SmartCareerPath.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data
{
    public static class PaymentSeeder
    {
        
        public static class PricingConfig
        {
            
            // USD Pricing (International)
            public static readonly Dictionary<ProductType, Dictionary<BillingCycle, decimal>> USDPricing = new()
        {
            {
                ProductType.InterviewerSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 9.99m },
                    { BillingCycle.Yearly, 99.99m }  // ~17% discount
                }
            },
            {
                ProductType.CVBuilderSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 6.99m },
                    { BillingCycle.Yearly, 69.99m }  // ~17% discount
                }
            },
            {
                ProductType.BundleSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 13.99m },  // Save $2.99/month
                    { BillingCycle.Yearly, 139.99m }   // ~18% discount
                }
            },
            {
                ProductType.InterviewerLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 199.99m }
                }
            },
            {
                ProductType.CVBuilderLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 149.99m }
                }
            },
            {
                ProductType.BundleLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 299.99m }  // Save $49.99
                }
            },
            {
                ProductType.SingleInterview, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.PayPerUse, 4.99m }
                }
            },
            {
                ProductType.SingleCV, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.PayPerUse, 2.99m }
                }
            }
        };

            
            // EGP Pricing (Egyptian Market)
            public static readonly Dictionary<ProductType, Dictionary<BillingCycle, decimal>> EGPPricing = new()
        {
            {
                ProductType.InterviewerSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 299.99m },
                    { BillingCycle.Yearly, 2999.99m }
                }
            },
            {
                ProductType.CVBuilderSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 199.99m },
                    { BillingCycle.Yearly, 1999.99m }
                }
            },
            {
                ProductType.BundleSubscription, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Monthly, 30m },
                    { BillingCycle.Yearly, 300m }
                }
            },
            {
                ProductType.InterviewerLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 5999.99m }
                }
            },
            {
                ProductType.CVBuilderLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 4499.99m }
                }
            },
            {
                ProductType.BundleLifetime, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.Lifetime, 8999.99m }
                }
            },
            {
                ProductType.SingleInterview, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.PayPerUse, 149.99m }
                }
            },
            {
                ProductType.SingleCV, new Dictionary<BillingCycle, decimal>
                {
                    { BillingCycle.PayPerUse, 89.99m }
                }
            }
        };

            
            // Helper Methods
            public static decimal GetPrice(ProductType productType, Currency currency, BillingCycle billingCycle)
            {
                var pricing = currency switch
                {
                    Currency.USD => USDPricing,
                    Currency.EGP => EGPPricing,
                    Currency.EUR => USDPricing, // Use USD pricing for EUR (convert later)
                    Currency.GBP => USDPricing,
                    Currency.SAR => EGPPricing, // Use EGP pricing as reference for SAR
                    _ => throw new ArgumentException($"Unsupported currency: {currency}")
                };

                if (pricing.TryGetValue(productType, out var cycles))
                {
                    if (cycles.TryGetValue(billingCycle, out var price))
                    {
                        return price;
                    }
                }

                throw new ArgumentException($"Price not configured for {productType} - {billingCycle}");
            }

         
            public static List<BillingCycle> GetAvailableBillingCycles(ProductType productType)
            {
                if (USDPricing.TryGetValue(productType, out var cycles))
                {
                    return cycles.Keys.ToList();
                }
                return new List<BillingCycle>();
            }

            
            public static decimal? GetYearlyDiscountPercentage(ProductType productType, Currency currency)
            {
                try
                {
                    var monthlyPrice = GetPrice(productType, currency, BillingCycle.Monthly);
                    var yearlyPrice = GetPrice(productType, currency, BillingCycle.Yearly);

                    var equivalentYearlyPrice = monthlyPrice * 12;
                    var discount = (equivalentYearlyPrice - yearlyPrice) / equivalentYearlyPrice * 100;

                    return Math.Round(discount, 0);
                }
                catch
                {
                    return null;
                }
            }
        }

      
        // Product Descriptions for Frontend
        public static class ProductInfo
        {
            public static readonly Dictionary<ProductType, (string Name, string Description, List<string> Features)> Products = new()
        {
            {
                ProductType.InterviewerSubscription,
                (
                    "AI Interviewer Pro",
                    "Master your interview skills with unlimited AI-powered practice sessions",
                    new List<string>
                    {
                        "✅ Unlimited interview sessions",
                        "✅ Technical, HR, and Behavioral interviews",
                        "✅ Real-time AI feedback and scoring",
                        "✅ Audio recording support",
                        "✅ Downloadable PDF reports",
                        "✅ Progress tracking over time",
                        "✅ Custom interview scenarios"
                    }
                )
            },
            {
                ProductType.CVBuilderSubscription,
                (
                    "Smart CV Builder",
                    "Create professional, ATS-friendly CVs powered by AI",
                    new List<string>
                    {
                        "✅ Unlimited CV generations",
                        "✅ AI-powered content improvement",
                        "✅ Multiple professional templates",
                        "✅ ATS optimization",
                        "✅ PDF & DOCX export",
                        "✅ Arabic & English support",
                        "✅ Role-specific customization"
                    }
                )
            },
            {
                ProductType.BundleSubscription,
                (
                    "Career Pro Bundle",
                    "Complete career toolkit - Interview prep + CV builder at a discount",
                    new List<string>
                    {
                        "✅ All Interviewer Pro features",
                        "✅ All CV Builder features",
                        "💰 Save up to $2.99/month",
                        "🎯 Best value for job seekers",
                        "⭐ Priority support",
                        "🔄 Seamless integration"
                    }
                )
            },
            {
                ProductType.InterviewerLifetime,
                (
                    "AI Interviewer - Lifetime Access",
                    "One-time payment for lifetime access to AI Interviewer",
                    new List<string>
                    {
                        "✅ All Pro features forever",
                        "✅ No recurring charges",
                        "✅ Future updates included",
                        "💎 Best long-term value",
                        "🔒 Locked-in pricing"
                    }
                )
            },
            {
                ProductType.CVBuilderLifetime,
                (
                    "CV Builder - Lifetime Access",
                    "One-time payment for lifetime CV builder access",
                    new List<string>
                    {
                        "✅ All features forever",
                        "✅ No recurring charges",
                        "✅ Future templates included",
                        "💎 Best long-term value",
                        "🔒 Locked-in pricing"
                    }
                )
            },
            {
                ProductType.BundleLifetime,
                (
                    "Career Pro Bundle - Lifetime",
                    "Complete toolkit for life - best investment in your career",
                    new List<string>
                    {
                        "✅ Both tools, lifetime access",
                        "✅ Save $49.99 vs separate",
                        "✅ No recurring charges ever",
                        "💎 Ultimate career investment",
                        "🔒 Locked-in pricing"
                    }
                )
            },
            {
                ProductType.SingleInterview,
                (
                    "Single Interview Session",
                    "Try our AI interviewer with one practice session",
                    new List<string>
                    {
                        "✅ One complete interview",
                        "✅ Full AI feedback report",
                        "✅ PDF export",
                        "🎯 Perfect to try before subscribing"
                    }
                )
            },
            {
                ProductType.SingleCV,
                (
                    "Single CV Generation",
                    "Generate one professional CV",
                    new List<string>
                    {
                        "✅ One AI-generated CV",
                        "✅ Choose any template",
                        "✅ PDF export",
                        "🎯 Perfect for immediate needs"
                    }
                )
            }
        };

            public static (string Name, string Description, List<string> Features) GetProductInfo(ProductType productType)
            {
                return Products.TryGetValue(productType, out var info)
                    ? info
                    : ("Unknown Product", "No description available", new List<string>());
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Test Data Seeds (for development/testing)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Seed test payment transactions (call this only in Development environment).
        /// </summary>
        public static void SeedTestPayments(ApplicationDbContext context)
        {
            if (context.PaymentTransactions.Any())
            {
                return; // Already seeded
            }

            // This would require User seeding first
            // Example structure - implement when User seeding is ready

            /*
            var testUser = context.Users.FirstOrDefault();
            if (testUser != null)
            {
                var testPayment = new PaymentTransaction
                {
                    UserId = testUser.Id,
                    Provider = PaymentProvider.Stripe,
                    ProviderReference = "test_" + Guid.NewGuid().ToString(),
                    Amount = 9.99m,
                    Currency = Currency.USD,
                    ProductType = ProductType.InterviewerSubscription,
                    Status = PaymentStatus.Completed,
                    BillingCycle = BillingCycle.Monthly,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                context.PaymentTransactions.Add(testPayment);
                context.SaveChanges();
            }
            */
        }
    }
    }