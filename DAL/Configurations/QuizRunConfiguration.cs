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
    public class QuizRunConfiguration : IEntityTypeConfiguration<QuizRun>
    {
        public void Configure(EntityTypeBuilder<QuizRun> builder)
        {
            builder.ToTable("QuizRuns");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Mode).IsRequired();
            builder.Property(x => x.StartedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.FinishedAt);

            builder.HasOne(x => x.WordList)
                   .WithMany()
                   .HasForeignKey(x => x.WordListId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WordListId, x.StartedAt });

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
