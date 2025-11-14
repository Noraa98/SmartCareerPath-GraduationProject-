using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.AIEngine
{
    public class AIResponseCache : BaseEntity
    {
        [Required, MaxLength(64)]
        public string HashKey { get; set; }

        [Required]
        public string ResponseText { get; set; }

        public int HitCount { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
