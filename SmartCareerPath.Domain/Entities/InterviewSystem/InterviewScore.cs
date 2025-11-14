using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.InterviewSystem
{
    public class InterviewScore : BaseEntity
    {
        [Required]
        public int InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; }

        public int OverallScore { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int ConfidenceScore { get; set; }

        public string Summary { get; set; }

        public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
    }

}
