using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.InterviewSystem
{
    public class InterviewKeyword : BaseEntity
    {
        [Required]
        public int InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; }

        [Required, MaxLength(100)]
        public string Keyword { get; set; }

        public int Frequency { get; set; }

        [MaxLength(50)]
        public string Sentiment { get; set; }
    }
}
