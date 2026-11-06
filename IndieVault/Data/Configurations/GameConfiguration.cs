using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Games");

            builder.Property(g => g.Title)
                   .IsRequired()
                   .HasMaxLength(100);


            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(g => g.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.ReleaseDate)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(g => g.CoverImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(g => g.DownloadLink)
               .HasMaxLength(500);

            builder.Property(g => g.IsFeatured)
               .IsRequired();

            builder.Property(g => g.CreatedDate)
               .HasColumnType("datetime")
               .IsRequired();

            builder.Property(g => g.DeveloperId)
               .IsRequired();

            builder.Property(g => g.IsFromExternalApi)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(g => g.ExternalApiSource)
                .HasMaxLength(100);

            builder.HasOne(g => g.Engine)
               .WithMany(e => e.Games)
               .HasForeignKey(g => g.EngineId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Genre)
               .WithMany(g => g.Games)
               .HasForeignKey(g => g.GenreId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Developer)
               .WithMany()
               .HasForeignKey(g => g.DeveloperId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.GamePlatforms)
               .WithOne(gp => gp.Game)
               .HasForeignKey(gp => gp.GameId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(g => g.GameTags)
                .WithOne(gt => gt.Game)
                .HasForeignKey(gt => gt.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(g => g.Reviews)
                .WithOne(r => r.Game)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Screenshots)
                .WithOne(r => r.Game)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(g => g.Title);
        }
    }
}
