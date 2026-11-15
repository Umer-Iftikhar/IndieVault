using IndieVault.Data;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Tests.TestHelpers
{
    // Fresh, isolated in-memory database per call (unique name per test).
    public static class InMemoryContext
    {
        public static AppDbContext Create(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
