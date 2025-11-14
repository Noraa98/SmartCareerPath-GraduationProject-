using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.AIEngine;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SubscriptionsAndBilling
{
    public class TokenUsage : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public int TokensUsed { get; set; }

        public int? RequestId { get; set; }
        public AIRequest Request { get; set; }

        [MaxLength(50)]
        public string RequestType { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
