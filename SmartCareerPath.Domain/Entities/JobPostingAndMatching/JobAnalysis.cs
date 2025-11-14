using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.JobPostingAndMatching
{
    public class JobAnalysis : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int JobPostingId { get; set; }
        public JobPosting JobPosting { get; set; }

        public int? ResumeId { get; set; }
        public Resume Resume { get; set; }

        public int MatchPercentage { get; set; }

        public string SkillGapsJson { get; set; }

        public string RecommendedActionsJson { get; set; }

        public string StrengthsJson { get; set; }

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}
