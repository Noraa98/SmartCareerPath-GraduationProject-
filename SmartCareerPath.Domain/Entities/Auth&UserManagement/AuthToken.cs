using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Auth
{
    public class AuthToken : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(500)]
        public string Token { get; set; }

        [MaxLength(500)]
        public string RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        [MaxLength(50)]
        public string DeviceInfo { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }
    }
}
