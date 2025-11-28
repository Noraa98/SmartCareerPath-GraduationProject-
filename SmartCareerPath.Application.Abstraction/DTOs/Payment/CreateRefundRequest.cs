using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class CreateRefundRequest
    {
        [Required]
        public int PaymentTransactionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal RefundAmount { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Please provide a reason for refund")]
        [MaxLength(1000)]
        public required string Reason { get; set; }
    }

}
