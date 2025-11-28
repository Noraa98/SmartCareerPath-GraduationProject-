using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class ReviewRefundRequest
    {
        [Required]
        public int RefundRequestId { get; set; }

        [Required]
        public int AdminId { get; set; }

        [Required]
        public bool Approve { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }
    }

}
