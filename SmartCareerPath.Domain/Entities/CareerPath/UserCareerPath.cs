using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class UserCareerPath : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int CareerPathId { get; set; }
        public CareerPath CareerPath { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public int ProgressPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastActivityAt { get; set; }

        // Navigation
        public ICollection<UserCareerPathProgress> Progress { get; set; } = new List<UserCareerPathProgress>();
        public ICollection<SkillGap> SkillGaps { get; set; } = new List<SkillGap>();
    }
}
