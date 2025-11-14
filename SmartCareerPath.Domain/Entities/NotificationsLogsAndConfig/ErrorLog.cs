using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig
{
    public class ErrorLog : BaseEntity
    {
        [MaxLength(500)]
        public string Endpoint { get; set; }

        [Required]
        public string ErrorMessage { get; set; }

        public string StackTrace { get; set; }

        [MaxLength(100)]
        public string UserId { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
