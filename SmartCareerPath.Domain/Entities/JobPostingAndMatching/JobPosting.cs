using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.JobPostingAndMatching
{
    public class JobPosting : BaseEntity
    {
        [Required]
        public int EmployerId { get; set; }
        public User Employer { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, MaxLength(200)]
        public string Company { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string SalaryRange { get; set; }

        [MaxLength(50)]
        public string JobType { get; set; }

        [MaxLength(50)]
        public string ExperienceLevel { get; set; }

        [MaxLength(500)]
        public string JobUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsRemote { get; set; }

        public DateTime PostedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public int ViewCount { get; set; }
        public int ApplicationCount { get; set; }

        // Navigation
        public ICollection<JobSkill> RequiredSkills { get; set; } = new List<JobSkill>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<JobAnalysis> Analyses { get; set; } = new List<JobAnalysis>();
    }
}
