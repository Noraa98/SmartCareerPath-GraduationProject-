using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Quiz
{
    public class QuizOption : BaseEntity
    {
        [Required]
        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; }

        [Required]
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public string Explanation { get; set; }
    }
}
