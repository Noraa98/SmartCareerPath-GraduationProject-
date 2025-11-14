using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class CareerPath : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public int EstimatedMonths { get; set; }

        [MaxLength(50)]
        public string Difficulty { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; }

        // Navigation
        public ICollection<CareerPathSkill> RequiredSkills { get; set; } = new List<CareerPathSkill>();
        public ICollection<CareerPathStep> Steps { get; set; } = new List<CareerPathStep>();
        public ICollection<UserCareerPath> UserCareerPaths { get; set; } = new List<UserCareerPath>();
    }
}
