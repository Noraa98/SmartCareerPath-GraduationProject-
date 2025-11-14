using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CareerPath
{
    public class UserCareerPathProgress : BaseEntity
    {
        [Required]
        public int UserCareerPathId { get; set; }
        public UserCareerPath UserCareerPath { get; set; }

        [Required]
        public int TaskId { get; set; }
        public CareerPathTask Task { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string Notes { get; set; }
    }
}
