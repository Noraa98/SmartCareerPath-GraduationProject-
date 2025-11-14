using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.InterviewSystem
{
    public class InterviewQuestion : BaseEntity
    {
        [Required]
        public int InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; }

        [Required]
        public string QuestionText { get; set; }

        public string UserAnswer { get; set; }

        [MaxLength(500)]
        public string AudioAnswerUrl { get; set; }

        public int? Score { get; set; }

        public string FeedbackJson { get; set; }

        public int OrderIndex { get; set; }

        public DateTime AskedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AnsweredAt { get; set; }

        public int? TimeSpentSeconds { get; set; }
    }
}
