using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class GameTagConfiguration : IEntityTypeConfiguration<GameTag>
    {
        public void Configure(EntityTypeBuilder<GameTag> builder)
        {
            builder.ToTable("GameTags");

            builder.HasKey(gt => new { gt.GameId, gt.TagId }); // Composite key.

            builder.HasOne(gt => gt.Game)
                .WithMany(g => g.GameTags)
                .HasForeignKey(gt => gt.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(gt => gt.Tag)
                .WithMany(t => t.GameTags)
                .HasForeignKey(gt => gt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}