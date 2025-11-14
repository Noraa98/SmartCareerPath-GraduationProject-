using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Quiz
{
    public class QuizConfiguration : IEntityTypeConfiguration<Domain.Entities.Quiz.Quiz>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Quiz.Quiz> builder)
        {
            builder.ToTable("Quizzes");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(q => q.Category)
                .HasMaxLength(100);

            builder.Property(q => q.Difficulty)
                .HasMaxLength(50);
        }
    }
}
