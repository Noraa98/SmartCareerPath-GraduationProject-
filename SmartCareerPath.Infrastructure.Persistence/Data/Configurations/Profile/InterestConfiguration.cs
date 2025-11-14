using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Profile
{
    public class InterestConfiguration : IEntityTypeConfiguration<Interest>
    {
        public void Configure(EntityTypeBuilder<Interest> builder)
        {
            builder.ToTable("Interests");

            builder.HasKey(i => i.Id);

            builder.HasIndex(i => i.Name);

            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(128);
        }
    }
}
