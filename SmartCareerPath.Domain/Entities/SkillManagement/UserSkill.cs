using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SkillManagement
{
    public class UserSkill : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public int ProficiencyLevel { get; set; }

        [MaxLength(50)]
        public string Source { get; set; }

        public bool IsVerified { get; set; }

        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastAssessedAt { get; set; }
    }

   
}
