using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Payments
{
    public class PaymentTransaction : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public int? SubscriptionId { get; set; }
        public UserSubscription Subscription { get; set; }

        [Required, MaxLength(50)]
        public string Provider { get; set; }

        [Required, MaxLength(200)]
        public string ProviderReference { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; }

        [Required, MaxLength(100)]
        public string ProductType { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(100)]
        public string PaymentMethod { get; set; }

        public string FailureReason { get; set; }

        public string MetadataJson { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
