using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig
{
    public class UserRecommendation : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        public int? RelatedEntityId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? MatchPercentage { get; set; }

        public string Reason { get; set; }

        public bool IsViewed { get; set; }

        public bool IsAccepted { get; set; }

        public DateTime? ViewedAt { get; set; }
    }
}
