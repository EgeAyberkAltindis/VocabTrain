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
    public class TurkishWordConfiguration : IEntityTypeConfiguration<TurkishWord>
    {
        public void Configure(EntityTypeBuilder<TurkishWord> builder)
        {
            builder.ToTable("TurkishWords");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.TextNormalized)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(x => x.TextNormalized).IsUnique();

            builder.Property(x => x.PartOfSpeech).HasMaxLength(50);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
