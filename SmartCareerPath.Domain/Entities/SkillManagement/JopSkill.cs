using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SkillManagement
{
    public class JobSkill : BaseEntity
    {
        [Required]
        public int JobPostingId { get; set; }
        public JobPosting JobPosting { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public bool IsRequired { get; set; }

        public int? MinimumLevel { get; set; }

        public int Priority { get; set; }
    }
}
