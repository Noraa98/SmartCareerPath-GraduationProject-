using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class SkillGap : BaseEntity
    {
        [Required]
        public int UserCareerPathId { get; set; }
        public UserCareerPath UserCareerPath { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public int CurrentLevel { get; set; }

        public int RequiredLevel { get; set; }

        public int GapSize { get; set; }

        public string RecommendedAction { get; set; }

        public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
