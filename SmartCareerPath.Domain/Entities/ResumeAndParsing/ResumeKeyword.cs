using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ResumeAndParsing
{
    public class ResumeKeyword : BaseEntity
    {
        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        [Required, MaxLength(100)]
        public string Keyword { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        public int Frequency { get; set; }

        public int Importance { get; set; }
    }
}
