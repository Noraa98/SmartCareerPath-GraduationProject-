using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig
{
    public class Notification : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [MaxLength(500)]
        public string ActionUrl { get; set; }

        public int? RelatedEntityId { get; set; }

        [MaxLength(50)]
        public string RelatedEntityType { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
