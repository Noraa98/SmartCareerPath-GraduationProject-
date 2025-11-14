using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class CareerPathSkill : BaseEntity
    {
        [Required]
        public int CareerPathId { get; set; }
        public CareerPath CareerPath { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public bool IsRequired { get; set; }

        public int RequiredLevel { get; set; }

        public int Priority { get; set; }
    }
}
