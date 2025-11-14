using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class CareerPathStep : BaseEntity
    {
        [Required]
        public int CareerPathId { get; set; }
        public CareerPath CareerPath { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int OrderIndex { get; set; }

        public int EstimatedWeeks { get; set; }

        // Navigation
        public ICollection<CareerPathTask> Tasks { get; set; } = new List<CareerPathTask>();
    }
}
