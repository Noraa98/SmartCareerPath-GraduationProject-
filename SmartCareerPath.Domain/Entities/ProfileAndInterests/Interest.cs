using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ProfileAndInterests
{
    public class Interest : BaseEntity
    {
        [Required, MaxLength(128)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    }
}
