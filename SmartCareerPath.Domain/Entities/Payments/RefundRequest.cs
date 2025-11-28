using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Entities.Payments
{
    public class RefundRequest : BaseEntity
    {
        
        [Required]
        public int PaymentTransactionId { get; set; }

       
        [Required]
        public int UserId { get; set; }

       
        [Required]
        public decimal RefundAmount { get; set; }

        
        [Required]
        public Currency Currency { get; set; }

      
        [Required]
        [MaxLength(1000)]
        public required string Reason { get; set; }

        
        [Required]
        public RefundStatus Status { get; set; } = RefundStatus.Requested;

        
        public int? ReviewedByAdminId { get; set; }

        
        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

       
        [MaxLength(200)]
        public string? ProviderRefundReference { get; set; }

        
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        
        public DateTime? ReviewedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        
        // Navigation Properties
        public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;

        
        public virtual User User { get; set; } = null!;

        
        public virtual User? ReviewedByAdmin { get; set; }


        // Business Logic Methods
        public void Approve(int adminId, string? adminNotes = null)
        {
            Status = RefundStatus.Approved;
            ReviewedByAdminId = adminId;
            AdminNotes = adminNotes;
            ReviewedAt = DateTime.UtcNow;
        }

       
        public void Reject(int adminId, string rejectionReason)
        {
            Status = RefundStatus.Rejected;
            ReviewedByAdminId = adminId;
            AdminNotes = rejectionReason;
            ReviewedAt = DateTime.UtcNow;
        }

 
        public void MarkAsCompleted(string providerReference)
        {
            Status = RefundStatus.Completed;
            ProviderRefundReference = providerReference;
            ProcessedAt = DateTime.UtcNow;
        }

     
        public void MarkAsFailed(string errorMessage)
        {
            Status = RefundStatus.Failed;
            ErrorMessage = errorMessage;
            ProcessedAt = DateTime.UtcNow;
        }

        
        public bool IsPendingReview()
        {
            return Status == RefundStatus.Requested || Status == RefundStatus.UnderReview;
        }

     
        public string GetDisplayAmount()
        {
            return Currency switch
            {
                Currency.USD => $"${RefundAmount:F2}",
                Currency.EUR => $"€{RefundAmount:F2}",
                Currency.GBP => $"£{RefundAmount:F2}",
                Currency.EGP => $"{RefundAmount:F2} EGP",
                Currency.SAR => $"{RefundAmount:F2} SAR",
                _ => $"{RefundAmount:F2} {Currency}"
            };
        }
    }
}
