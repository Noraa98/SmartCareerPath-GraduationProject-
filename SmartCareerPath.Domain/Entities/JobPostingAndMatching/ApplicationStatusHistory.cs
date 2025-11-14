using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.JobPostingAndMatching
{
    public class ApplicationStatusHistory : BaseEntity
    {
        [Required]
        public int JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        public string Notes { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public int? ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; }
    }
}
