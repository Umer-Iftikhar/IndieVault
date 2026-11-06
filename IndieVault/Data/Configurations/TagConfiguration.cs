using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.Property(p => p.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
