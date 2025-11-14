using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.AIEngine
{
    public class AIRequest : BaseEntity
    {
        public int? UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        public string InputText { get; set; }

        public string OutputText { get; set; }

        [MaxLength(100)]
        public string ModelName { get; set; }

        public int? TokensUsed { get; set; }

        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }

        public int? ResponseTimeMs { get; set; }
    }
}
