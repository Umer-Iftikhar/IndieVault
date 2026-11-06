using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndieVault.Data.Configurations
{
    public class DownloadHistoryConfiguration : IEntityTypeConfiguration<DownloadHistory>
    {
        public void Configure(EntityTypeBuilder<DownloadHistory> builder)
        {
            builder.ToTable("DownloadHistories");

            builder.Property(dh => dh.DownloadDate)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(dh => dh.UserId)
                .IsRequired();

            builder.HasOne(dh => dh.User)
                .WithMany()
                .HasForeignKey(dh => dh.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
