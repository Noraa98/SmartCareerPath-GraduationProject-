using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class CareerPathTask : BaseEntity
    {
        [Required]
        public int StepId { get; set; }
        public CareerPathStep Step { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string ResourceUrl { get; set; }

        [MaxLength(50)]
        public string ResourceType { get; set; }

        public int OrderIndex { get; set; }

        public int EstimatedHours { get; set; }

        public bool IsOptional { get; set; }
    }

}
