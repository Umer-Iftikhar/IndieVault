using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class ScreenshotConfiguration : IEntityTypeConfiguration<Screenshot>
    {
        public void Configure(EntityTypeBuilder<Screenshot> builder)
        {
            builder.ToTable("Screenshots");

            builder.Property(s => s.ImagePath)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}
