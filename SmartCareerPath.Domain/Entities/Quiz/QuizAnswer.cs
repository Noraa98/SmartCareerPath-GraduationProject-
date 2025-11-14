using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Quiz
{
    public class QuizAnswer : BaseEntity
    {
        [Required]
        public int QuizSessionId { get; set; }
        public QuizSession QuizSession { get; set; }

        [Required]
        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; }

        [MaxLength(500)]
        public string SelectedOptionIds { get; set; }

        public string TextAnswer { get; set; }

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        public bool IsCorrect { get; set; }

        public int PointsEarned { get; set; }
    }
}
