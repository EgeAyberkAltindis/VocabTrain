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
    public class ExampleSentenceConfiguration : IEntityTypeConfiguration<ExampleSentence>
    {
        public void Configure(EntityTypeBuilder<ExampleSentence> builder)
        {
            builder.ToTable("ExampleSentences");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EnglishText)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.OrderIndex)
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.HasOne(x => x.EnglishWord)
                   .WithMany(w => w.Sentences)
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.EnglishWordId, x.EnglishText }).IsUnique();

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
