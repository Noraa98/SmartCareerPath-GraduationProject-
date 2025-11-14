using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Auth
{
    public class AuthTokenConfiguration : IEntityTypeConfiguration<AuthToken>
    {
        public void Configure(EntityTypeBuilder<AuthToken> builder)
        {
            builder.ToTable("AuthTokens");

            builder.HasKey(t => t.Id);

            builder.HasIndex(t => t.Token);
            builder.HasIndex(t => t.RefreshToken);

            builder.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.RefreshToken)
                .HasMaxLength(500);

            builder.HasOne(t => t.User)
                .WithMany(u => u.AuthTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
