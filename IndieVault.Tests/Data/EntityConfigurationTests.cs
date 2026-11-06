using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IndieVault.Tests.Data
{
    // Verifies the Fluent API configurations in Data/Configurations.
    // These rules used to be data annotations on the models.
    public class EntityConfigurationTests : IDisposable
    {
        private readonly Microsoft.EntityFrameworkCore.DbContext _context;
        private readonly IModel _model;

        public EntityConfigurationTests()
        {
            _context = MySqlModelContext.Create();
            _model = _context.GetService<IDesignTimeModel>().Model;
        }

        public void Dispose() => _context.Dispose();

        private IEntityType Entity<T>() => _model.FindEntityType(typeof(T))!;

        [Theory]
        [InlineData(typeof(Game), "Games")]
        [InlineData(typeof(Genre), "Genres")]
        [InlineData(typeof(Engine), "Engines")]
        [InlineData(typeof(Platform), "Platforms")]
        [InlineData(typeof(Tag), "Tags")]
        [InlineData(typeof(GamePlatform), "GamePlatforms")]
        [InlineData(typeof(GameTag), "GameTags")]
        [InlineData(typeof(Screenshot), "Screenshots")]
        [InlineData(typeof(Review), "Reviews")]
        [InlineData(typeof(Wishlist), "Wishlists")]
        [InlineData(typeof(DownloadHistory), "DownloadHistories")]
        public void Entities_MapToExpectedTables(Type clrType, string expectedTable)
        {
            var entity = _model.FindEntityType(clrType);

            Assert.NotNull(entity);
            Assert.Equal(expectedTable, entity!.GetTableName());
        }

        [Theory]
        [InlineData(nameof(Game.Title), 100)]
        [InlineData(nameof(Game.Description), 500)]
        [InlineData(nameof(Game.CoverImagePath), 500)]
        [InlineData(nameof(Game.DownloadLink), 500)]
        [InlineData(nameof(Game.ExternalApiSource), 100)]
        public void Game_StringColumns_HaveMaxLength(string propertyName, int maxLength)
        {
            var property = Entity<Game>().FindProperty(propertyName)!;

            Assert.Equal(maxLength, property.GetMaxLength());
        }

        [Theory]
        [InlineData(nameof(Game.Title))]
        [InlineData(nameof(Game.Description))]
        [InlineData(nameof(Game.CoverImagePath))]
        [InlineData(nameof(Game.DeveloperId))]
        public void Game_RequiredColumns_AreNotNullable(string propertyName)
        {
            Assert.False(Entity<Game>().FindProperty(propertyName)!.IsNullable);
        }

        [Fact]
        public void Game_DownloadLink_IsNullable()
        {
            // RAWG games without a store have no download link
            Assert.True(Entity<Game>().FindProperty(nameof(Game.DownloadLink))!.IsNullable);
        }

        [Fact]
        public void Game_Price_IsDecimal18_2()
        {
            Assert.Equal("decimal(18,2)", Entity<Game>().FindProperty(nameof(Game.Price))!.GetColumnType());
        }

        [Fact]
        public void Game_IsFromExternalApi_DefaultsToFalse()
        {
            Assert.Equal(false, Entity<Game>().FindProperty(nameof(Game.IsFromExternalApi))!.GetDefaultValue());
        }

        [Fact]
        public void Game_Title_IsIndexed()
        {
            Assert.Contains(Entity<Game>().GetIndexes(),
                i => i.Properties.Count == 1 && i.Properties[0].Name == nameof(Game.Title));
        }

        [Theory]
        [InlineData(typeof(Engine), DeleteBehavior.Restrict)]
        [InlineData(typeof(Genre), DeleteBehavior.Restrict)]
        [InlineData(typeof(ApplicationUser), DeleteBehavior.Cascade)]
        public void Game_ForeignKeys_HaveExpectedDeleteBehavior(Type principal, DeleteBehavior expected)
        {
            var fk = Entity<Game>().GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == principal);

            Assert.Equal(expected, fk.DeleteBehavior);
        }

        [Fact]
        public void Review_IsDeletedWithItsGame()
        {
            var fk = Entity<Review>().GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Game));

            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }

        [Fact]
        public void Review_Comment_IsOptionalAndMax1000()
        {
            var comment = Entity<Review>().FindProperty(nameof(Review.Comment))!;

            Assert.True(comment.IsNullable);
            Assert.Equal(1000, comment.GetMaxLength());
        }

        [Theory]
        [InlineData(typeof(Genre), 100)]
        [InlineData(typeof(Engine), 50)]
        [InlineData(typeof(Platform), 50)]
        [InlineData(typeof(Tag), 50)]
        public void LookupTables_NameIsRequiredUniqueAndLimited(Type clrType, int maxLength)
        {
            var entity = _model.FindEntityType(clrType)!;
            var name = entity.FindProperty("Name")!;
            var index = entity.GetIndexes().Single(i => i.Properties.Count == 1 && i.Properties[0].Name == "Name");

            Assert.False(name.IsNullable);
            Assert.Equal(maxLength, name.GetMaxLength());
            Assert.True(index.IsUnique);
        }

        [Theory]
        [InlineData(typeof(GamePlatform), "GameId", "PlatformId")]
        [InlineData(typeof(GameTag), "GameId", "TagId")]
        public void JoinTables_UseCompositePrimaryKey(Type clrType, string first, string second)
        {
            var key = _model.FindEntityType(clrType)!.FindPrimaryKey()!;

            Assert.Equal(new[] { first, second }, key.Properties.Select(p => p.Name));
        }

        [Theory]
        [InlineData(typeof(Review), "ReviewDate")]
        [InlineData(typeof(Wishlist), "CreatedDate")]
        [InlineData(typeof(DownloadHistory), "DownloadDate")]
        public void ActivityDates_UseDatetimeColumns(Type clrType, string propertyName)
        {
            var property = _model.FindEntityType(clrType)!.FindProperty(propertyName)!;

            Assert.Equal("datetime", property.GetColumnType());
            Assert.False(property.IsNullable);
        }

        [Fact]
        public void Screenshot_ImagePath_IsRequiredMax500()
        {
            var imagePath = Entity<Screenshot>().FindProperty(nameof(Screenshot.ImagePath))!;

            Assert.False(imagePath.IsNullable);
            Assert.Equal(500, imagePath.GetMaxLength());
        }
    }
}
