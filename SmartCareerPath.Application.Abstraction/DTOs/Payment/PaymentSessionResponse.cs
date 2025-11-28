using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PaymentSessionResponse
    {
        public int TransactionId { get; set; }
        public required string ProviderReference { get; set; }
        public required string CheckoutUrl { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string ProductType { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
