using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PricingTierResponse
    {
        public required string BillingCycle { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string DisplayAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? DiscountLabel { get; set; }
    }
}
