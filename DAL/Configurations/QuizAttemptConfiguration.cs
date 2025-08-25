using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configurations
{
    public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.ToTable("QuizAttempts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsCorrect).IsRequired();

            builder.HasOne(x => x.QuizRun)
                   .WithMany()
                   .HasForeignKey(x => x.QuizRunId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.EnglishWord)
                   .WithMany()
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.QuizRunId);
            builder.HasIndex(x => new { x.EnglishWordId, x.CreatedAt });

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
