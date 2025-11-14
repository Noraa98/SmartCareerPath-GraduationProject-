using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig
{
    public class SystemConfig : BaseEntity
    {
        [Required, MaxLength(100)]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string UpdatedBy { get; set; }
    }
}
