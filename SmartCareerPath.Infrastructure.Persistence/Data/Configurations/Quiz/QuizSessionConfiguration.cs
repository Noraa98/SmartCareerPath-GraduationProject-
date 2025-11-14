using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Quiz
{
    public class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
    {
        public void Configure(EntityTypeBuilder<QuizSession> builder)
        {
            builder.ToTable("QuizSessions");

            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.SessionId)
                .IsUnique();

            builder.HasOne(s => s.User)
                .WithMany(u => u.QuizSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Quiz)
                .WithMany(q => q.Sessions)
                .HasForeignKey(s => s.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
