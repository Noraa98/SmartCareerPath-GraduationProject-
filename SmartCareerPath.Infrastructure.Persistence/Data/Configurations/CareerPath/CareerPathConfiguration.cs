using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.CareerPath
{
    public class CareerPathConfiguration : IEntityTypeConfiguration<Domain.Entities.CareerPath.CareerPath>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.CareerPath.CareerPath> builder)
        {
            builder.ToTable("CareerPaths");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Category)
                .HasMaxLength(100);

            builder.Property(c => c.Difficulty)
                .HasMaxLength(50);
        }
    }

}
