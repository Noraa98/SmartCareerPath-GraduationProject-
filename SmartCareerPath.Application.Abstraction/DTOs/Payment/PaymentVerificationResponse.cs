using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PaymentVerificationResponse
    {
        public int TransactionId { get; set; }
        public required string Status { get; set; }
        public int? SubscriptionId { get; set; }
        public required string Message { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
