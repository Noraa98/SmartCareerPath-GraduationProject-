using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.CareerPath;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SkillManagement
{
    public class Skill : BaseEntity
    {
        [Required, MaxLength(128)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(64)]
        public string Category { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<QuizSkill> QuizSkills { get; set; } = new List<QuizSkill>();
        public ICollection<CareerPathSkill> CareerPathSkills { get; set; } = new List<CareerPathSkill>();
    }
}
