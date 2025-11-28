using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.Payments;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SubscriptionsAndBilling
{
    public class UserSubscription : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public bool AutoRenew { get; set; } = true;

        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        public int AIRequestsUsed { get; set; }
        public int ResumeParsingUsed { get; set; }
        public int QuizAttemptsUsed { get; set; }
        public int MockInterviewsUsed { get; set; }
        public int JobApplicationsUsed { get; set; }

        // Navigation
        public ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
    }
}
