using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class RefundRequestResponse
    {
        public int Id { get; set; }
        public int PaymentTransactionId { get; set; }
        public int UserId { get; set; }
        public decimal RefundAmount { get; set; }
        public required string Currency { get; set; }
        public required string DisplayAmount { get; set; }
        public required string Reason { get; set; }
        public required string Status { get; set; }
        public int? ReviewedByAdminId { get; set; }
        public string? ReviewedByAdminName { get; set; }
        public string? AdminNotes { get; set; }
        public string? ProviderRefundReference { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

}
