using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CVTemplatesAndExport
{
    public class CVTemplate : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Locale { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string TemplateFileReference { get; set; }

        [MaxLength(500)]
        public string PreviewImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPremium { get; set; }

        public int UsageCount { get; set; }
    }

}
