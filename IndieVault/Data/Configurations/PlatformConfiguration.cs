using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class PlatformConfiguration : IEntityTypeConfiguration<Platform>
    {
        public void Configure(EntityTypeBuilder<Platform> builder)
        {
            builder.ToTable("Platforms");

            builder.Property(p => p.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
