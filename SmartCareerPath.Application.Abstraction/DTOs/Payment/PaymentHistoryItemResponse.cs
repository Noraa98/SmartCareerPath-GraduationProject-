using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PaymentHistoryItemResponse
    {
        public int TransactionId { get; set; }
        public required string ProductType { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string DisplayAmount { get; set; }
        public required string Status { get; set; }
        public required string Provider { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ReceiptUrl { get; set; }
    }

}
