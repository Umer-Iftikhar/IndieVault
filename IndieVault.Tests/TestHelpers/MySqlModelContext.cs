using IndieVault.Data;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Tests.TestHelpers
{
    // Builds an AppDbContext configured for MySQL without opening a connection.
    // Good enough to inspect the EF model (column types, keys, indexes, migrations).
    public static class MySqlModelContext
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql("Server=localhost;Database=indievault_model;User=test;Password=test;",
                    new MySqlServerVersion(new Version(8, 0, 31)))
                .Options;

            return new AppDbContext(options);
        }
    }
}
