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
    public class EnglishWordRelationConfiguration : IEntityTypeConfiguration<EnglishWordRelation>
    {
        public void Configure(EntityTypeBuilder<EnglishWordRelation> builder)
        {
            builder.ToTable("EnglishWordRelations");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.EnglishWord)
                   .WithMany(w => w.RelationsFrom)
                   .HasForeignKey(x => x.EnglishWordId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RelatedEnglishWord)
                   .WithMany(w => w.RelationsTo)
                   .HasForeignKey(x => x.RelatedEnglishWordId)
                   .OnDelete(DeleteBehavior.Restrict); // çift taraflı cascade zinciri olmasın

            // A–B ve B–A'nın aynı kabul edilmesi için benzersizlik (type yok artık)
            builder.HasIndex(x => new { x.EnglishWordId, x.RelatedEnglishWordId })
                   .IsUnique();

            // Simetrik duplikasyonu tamamen önlemek için tek yön kuralı:
            builder.HasCheckConstraint("CK_EnglishWordRelations_Order",
                "[EnglishWordId] < [RelatedEnglishWordId]");

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt);
        }
    }
}
