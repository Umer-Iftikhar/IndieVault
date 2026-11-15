using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {
            builder.ToTable("Genres");

            builder.Property(gen => gen.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(gen => gen.Name)
                .IsUnique();
        }
    }
}
