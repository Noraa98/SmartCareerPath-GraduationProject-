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
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(64);

            // Seed Data
            builder.HasData(
      new Role { Id = 1, Name = "User", Description = "Regular user", CreatedAt = new DateTime(2025, 1, 1) },
      new Role { Id = 2, Name = "Employer", Description = "Job poster", CreatedAt = new DateTime(2025, 1, 1) },
      new Role { Id = 3, Name = "Admin", Description = "System administrator", CreatedAt = new DateTime(2025, 1, 1) }
  );

        }
    }
}
