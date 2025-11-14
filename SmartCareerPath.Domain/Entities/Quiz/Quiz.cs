using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Quiz
{
    public class Quiz : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(50)]
        public string Difficulty { get; set; }

        public int TimeLimit { get; set; }

        public int PassingScore { get; set; }

        public bool IsPublished { get; set; }

        // Navigation
        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public ICollection<QuizSession> Sessions { get; set; } = new List<QuizSession>();
        public ICollection<QuizSkill> Skills { get; set; } = new List<QuizSkill>();
    }
}
