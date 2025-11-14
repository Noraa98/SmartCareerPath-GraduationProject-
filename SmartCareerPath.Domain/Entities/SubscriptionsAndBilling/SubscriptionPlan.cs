using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SubscriptionsAndBilling
{
    public class SubscriptionPlan : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";

        public string Description { get; set; }

        public int MonthlyAIRequestsLimit { get; set; }
        public int MonthlyResumeParsingLimit { get; set; }
        public int MonthlyQuizAttemptsLimit { get; set; }
        public int MonthlyMockInterviewsLimit { get; set; }
        public int MonthlyJobApplicationsLimit { get; set; }

        public bool HasAdvancedAI { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasCareerPathAccess { get; set; }
        public bool HasPremiumTemplates { get; set; }

        public bool IsActive { get; set; } = true;

        public int DurationMonths { get; set; } = 1;

        public int DisplayOrder { get; set; }

        // Navigation
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
