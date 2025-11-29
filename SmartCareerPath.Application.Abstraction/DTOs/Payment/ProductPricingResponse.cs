using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class ProductPricingResponse
    {
        public int ProductTypeId { get; set; }
        public required string ProductType { get; set; }
        public required string DisplayName { get; set; }
        public required string Description { get; set; }
        public List<PricingTierResponse> Tiers { get; set; } = new();
        public List<string> Features { get; set; } = new();
    }

}
