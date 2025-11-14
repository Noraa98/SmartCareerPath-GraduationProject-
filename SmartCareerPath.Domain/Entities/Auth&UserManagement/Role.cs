using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Auth
{
    public class Role : BaseEntity
    {
        [Required, MaxLength(64)]
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
