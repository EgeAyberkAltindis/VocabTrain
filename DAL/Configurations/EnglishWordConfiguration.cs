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
    public class EnglishWordConfiguration : IEntityTypeConfiguration<EnglishWord>
    {
        public void Configure(EntityTypeBuilder<EnglishWord> builder)
        {
            builder.ToTable("EnglishWords");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.TextNormalized)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(x => x.TextNormalized).IsUnique();

            builder.Property(x => x.PartOfSpeech).HasMaxLength(50);
            builder.Property(x => x.Level).HasMaxLength(20);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
