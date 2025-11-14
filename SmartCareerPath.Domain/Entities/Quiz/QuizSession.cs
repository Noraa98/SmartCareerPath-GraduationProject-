using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Quiz
{
    public class QuizSession : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public Guid SessionId { get; set; } = Guid.NewGuid();

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public int Percentage { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsPassed { get; set; }

        public int? TimeSpentSeconds { get; set; }

        // Navigation
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
