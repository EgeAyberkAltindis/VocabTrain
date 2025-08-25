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
    public class WordTranslationConfiguration : IEntityTypeConfiguration<WordTranslation>
    {
        public void Configure(EntityTypeBuilder<WordTranslation> builder)
        {
            builder.ToTable("WordTranslations");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsPrimary).HasDefaultValue(false);

            builder.HasOne(x => x.EnglishWord)
                   .WithMany(w => w.Translations)
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TurkishWord)
                   .WithMany(w => w.Translations)
                   .HasForeignKey(x => x.TurkishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.EnglishWordId, x.TurkishWordId }).IsUnique();

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
