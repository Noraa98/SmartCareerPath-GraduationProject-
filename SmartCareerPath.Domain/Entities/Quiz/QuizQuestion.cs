using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Quiz
{
    public class QuizQuestion : BaseEntity
    {
        [Required]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required, MaxLength(50)]
        public string QuestionType { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public int Points { get; set; } = 1;

        public string Explanation { get; set; }

        [MaxLength(500)]
        public string CodeSnippet { get; set; }

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
