using IndieVault.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IndieVault.Tests.Data
{
    public class MigrationsTests
    {
        [Fact]
        public void Model_HasNoChangesMissingFromMigrations()
        {
            using var context = MySqlModelContext.Create();

            // Fails if someone changes a model or configuration without adding a migration
            Assert.False(context.Database.HasPendingModelChanges());
        }

        [Fact]
        public void Migrations_AreDiscoveredInOrder()
        {
            using var context = MySqlModelContext.Create();

            var migrations = context.Database.GetMigrations().ToList();

            Assert.Equal(8, migrations.Count);
            Assert.EndsWith("_InitialCreate", migrations.First());
            Assert.EndsWith("_RemoveDataAnnotations", migrations.Last());
        }
    }
}
