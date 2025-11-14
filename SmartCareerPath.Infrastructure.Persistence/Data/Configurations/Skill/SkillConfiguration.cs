using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Skill
{
    public class SkillConfiguration : IEntityTypeConfiguration<Domain.Entities.SkillManagement.Skill>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.SkillManagement.Skill> builder)
        {
            builder.ToTable("Skills");

            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.Name);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(s => s.Category)
                .HasMaxLength(64);
        }
    }
}
