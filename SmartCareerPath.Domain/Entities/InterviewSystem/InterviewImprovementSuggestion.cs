using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.InterviewSystem
{
    public class InterviewImprovementSuggestion : BaseEntity
    {
        [Required]
        public int InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public string Suggestion { get; set; }

        public int Priority { get; set; }
    }

    

}
