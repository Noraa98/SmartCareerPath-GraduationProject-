using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ResumeAndParsing
{
    public class ResumeScore : BaseEntity
    {
        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        public int ATSScore { get; set; }

        public int ReadabilityScore { get; set; }

        public int SkillsRelevanceScore { get; set; }

        public int OverallScore { get; set; }

        public string Notes { get; set; }

        public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
    }
}
