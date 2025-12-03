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
        
        /// <summary>
        /// User ID associated with the payment (for token generation)
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// New JWT token with updated user role (issued after payment completion)
        /// Frontend should store this token immediately to get updated role/permissions
        /// </summary>
        public string? Token { get; set; }
    }
}
