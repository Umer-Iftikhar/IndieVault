using IndieVault.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IndieVault.Data
{
    public class AppDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>
    {
        public  AppDbContext (DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Wishlist> Wishlists { get; set; } = null!;
        public DbSet<DownloadHistory> DownloadHistories { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Screenshot> Screenshots { get; set; } = null!;

        // --- New 3NF Tables ---
        public DbSet<Engine> Engines { get; set; } = null!;
        public DbSet<Platform> Platforms { get; set; } = null!;
        public DbSet<GamePlatform> GamePlatforms { get; set; } = null!;
        public DbSet<GameTag> GameTags { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            /*Assembly.GetExecutingAssembly() — gets the current project's assembly (IndieVault project compiled into a .dll).
            ApplyConfigurationsFromAssembly(...) — scans that assembly, finds every class that implements IEntityTypeConfiguration<T>, and calls Configure() on each one automatically.*/
        }
    }
}
