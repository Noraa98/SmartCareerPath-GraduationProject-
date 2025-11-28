using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class VerifyPaymentRequest
    {
        [Required]
        [MaxLength(200)]
        public required string ProviderReference { get; set; }

        
        public string? Signature { get; set; }

      
        public string? WebhookPayload { get; set; }
    }
}
