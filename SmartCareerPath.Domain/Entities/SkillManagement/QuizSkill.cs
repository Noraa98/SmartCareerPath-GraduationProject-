using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.SkillManagement
{
    public class QuizSkill : BaseEntity
    {
        [Required]
        public int QuizId { get; set; }
        public Quiz.Quiz Quiz { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public int Weight { get; set; }
    }
}
