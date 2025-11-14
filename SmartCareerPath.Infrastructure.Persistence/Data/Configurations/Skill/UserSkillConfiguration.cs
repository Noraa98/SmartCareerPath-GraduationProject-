using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Skill
{
    public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
    {
        public void Configure(EntityTypeBuilder<UserSkill> builder)
        {
            builder.ToTable("UserSkills");

            builder.HasKey(us => us.Id);

            builder.HasIndex(us => new { us.UserId, us.SkillId })
                .IsUnique();

            builder.Property(us => us.ProficiencyLevel)
                .IsRequired();

            builder.HasOne(us => us.User)
                .WithMany(u => u.Skills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
