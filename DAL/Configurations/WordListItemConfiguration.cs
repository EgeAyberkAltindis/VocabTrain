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
    public class WordListItemConfiguration : IEntityTypeConfiguration<WordListItem>
    {
        public void Configure(EntityTypeBuilder<WordListItem> builder)
        {
            builder.ToTable("WordListItems");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.EnglishWord)
                   .WithMany(w => w.WordListItems)
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.WordList)
                   .WithMany(l => l.Items)
                   .HasForeignKey(x => x.WordListId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.WordListId, x.EnglishWordId }).IsUnique();

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
