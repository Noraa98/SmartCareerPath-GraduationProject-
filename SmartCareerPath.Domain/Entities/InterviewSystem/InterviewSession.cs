using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.InterviewSystem
{
    public class InterviewSession : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public int? JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; }

        public Guid SessionId { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string CareerRole { get; set; }

        [MaxLength(50)]
        public string InterviewType { get; set; }

        public int TotalQuestions { get; set; }
        public int CompletedQuestions { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public ICollection<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();
        public InterviewScore Score { get; set; }
        public ICollection<InterviewImprovementSuggestion> Improvements { get; set; } = new List<InterviewImprovementSuggestion>();
        public ICollection<InterviewKeyword> Keywords { get; set; } = new List<InterviewKeyword>();
    }

}
