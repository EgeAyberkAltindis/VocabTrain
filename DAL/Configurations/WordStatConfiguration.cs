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
    public class WordStatConfiguration : IEntityTypeConfiguration<WordStat>
    {
        public void Configure(EntityTypeBuilder<WordStat> builder)
        {
            builder.ToTable("WordStats");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TimesShown).HasDefaultValue(0);
            builder.Property(x => x.CorrectCount).HasDefaultValue(0);
            builder.Property(x => x.WrongCount).HasDefaultValue(0);

            builder.HasOne(x => x.WordList)
                   .WithMany()
                   .HasForeignKey(x => x.WordListId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.EnglishWord)
                   .WithMany()
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.WordListId, x.EnglishWordId }).IsUnique();

            builder.HasIndex(x => x.TimesShown);
            builder.HasIndex(x => x.CorrectCount);
            builder.HasIndex(x => x.WrongCount);
            builder.HasIndex(x => x.LastShownAt);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
