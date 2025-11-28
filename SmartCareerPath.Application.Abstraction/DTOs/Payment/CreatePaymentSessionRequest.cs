using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class CreatePaymentSessionRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 11, ErrorMessage = "Invalid product type")]
        public int ProductType { get; set; } // Enum: ProductType

        [Required]
        [Range(1, 3, ErrorMessage = "Invalid payment provider")]
        public int PaymentProvider { get; set; } // Enum: PaymentProvider

        [Required]
        [Range(1, 5, ErrorMessage = "Invalid currency")]
        public int Currency { get; set; } // Enum: Currency

        [Range(1, 3, ErrorMessage = "Invalid billing cycle")]
        public int? BillingCycle { get; set; } // Enum: BillingCycle (null for one-time)

        [MaxLength(50)]
        public string? DiscountCode { get; set; }

        
        [Url]
        public string? SuccessUrl { get; set; }

       
        [Url]
        public string? CancelUrl { get; set; }
    }
}
