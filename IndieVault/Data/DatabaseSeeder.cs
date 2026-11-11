using Bogus;
using IndieVault.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Data
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRoles(roleManager);
            var (devIds, playerIds) = await SeedUsersAsync(userManager);

            var genres = await SeedGenres(context);
            var engines = await SeedEngines(context);
            var platforms = await SeedPlatforms(context);
            var tags = await SeedTags(context);
            var games = await SeedGamesAsync(context, devIds, genres, engines);
            await SeedGamePlatformsAsync(context, games, platforms);
            await SeedGameTagsAsync(context, tags, games);
            await SeedReviewsAsync(context, games, playerIds);
            await SeedWishlistsAsync(context, games, playerIds);
            await SeedDownloadHistoriesAsync(context, games, playerIds);
            await SeedScreenshotsAsync(context, games);
        }

        public static async Task<List<Genre>> SeedGenres(AppDbContext context)
        {
            var existingGenres = await context.Genres.ToListAsync();

            // If nothing exists at all → seed everything including Uncategorized
            if (existingGenres.Count == 0)
            {
                var genres = new List<Genre>
        {
            new Genre { Name = "Action" },
            new Genre { Name = "Adventure" },
            new Genre { Name = "RPG" },
            new Genre { Name = "Strategy" },
            new Genre { Name = "Simulation" },
            new Genre { Name = "Puzzle" },
            new Genre { Name = "Horror" },
            new Genre { Name = "Sports" },
            new Genre { Name = "Racing" },
            new Genre { Name = "Uncategorized" }
        };

                await context.Genres.AddRangeAsync(genres);
                await context.SaveChangesAsync();

                return genres;
            }

            // If data exists → only ensure "Uncategorized"
            if (!existingGenres.Any(g => g.Name == "Uncategorized"))
            {
                var uncategorized = new Genre { Name = "Uncategorized" };
                await context.Genres.AddAsync(uncategorized);
                await context.SaveChangesAsync();

                existingGenres.Add(uncategorized);
            }

            return existingGenres;
        }
        public static async Task<List<Engine>> SeedEngines(AppDbContext context)
        {
            var existingEngines = await context.Engines.ToListAsync();

            // If nothing exists → seed everything including Unknown
            if (existingEngines.Count == 0)
            {
                var engines = new List<Engine>
        {
            new Engine { Name = "Unity" },
            new Engine { Name = "Unreal Engine" },
            new Engine { Name = "Godot" },
            new Engine { Name = "CryEngine" },
            new Engine { Name = "RPG Maker" },
            new Engine { Name = "Unknown" }
        };

                await context.Engines.AddRangeAsync(engines);
                await context.SaveChangesAsync();

                return engines;
            }

            // If data exists → only ensure "Unknown"
            if (!existingEngines.Any(e => e.Name == "Unknown"))
            {
                var unknown = new Engine { Name = "Unknown" };
                await context.Engines.AddAsync(unknown);
                await context.SaveChangesAsync();

                existingEngines.Add(unknown);
            }

            return existingEngines;
        }
        public static async Task<List<Platform>> SeedPlatforms(AppDbContext context)
        { 
            if (await context.Platforms.AnyAsync())
            {
                return await context.Platforms.ToListAsync();
            }
            var platforms = new List<Platform>
            {
                new Platform { Name = "Windows" },
                new Platform { Name = "macOS" },
                new Platform { Name = "Linux" },
                new Platform { Name = "PlayStation" },
                new Platform { Name = "Mobile (iOS/Android)" }
            };
            await context.Platforms.AddRangeAsync(platforms);
            await context.SaveChangesAsync();
            return platforms;
        }
        public static async Task<List<Tag>> SeedTags(AppDbContext context)
        {
            if (await context.Tags.AnyAsync())
            {
                return await context.Tags.ToListAsync();
            }
            var tags = new List<Tag>
            {
                new Tag { Name = "Multiplayer" },
                new Tag { Name = "Singleplayer" },
                new Tag { Name = "2D" },
                new Tag { Name = "Open World" },
                new Tag { Name = "Pixel Art" },
                new Tag { Name = "3D" },
                new Tag { Name = "Sandbox" }
            };
            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
            return tags;
        }
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "GameDev", "Player" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task<(List<string> devIds, List<string> playerIds)> SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            const string defaultPassword = "Password123!";
            var devIds = new List<string>();
            var playerIds = new List<string>();

            var existingDevs = await userManager.GetUsersInRoleAsync("GameDev");
            var existingPlayers = await userManager.GetUsersInRoleAsync("Player");
            if (existingDevs.Any())
            {
                return (existingDevs.Select(u => u.Id).ToList(), existingPlayers.Select(u => u.Id).ToList());
            }

            var adminEmail = "admin@indiehub.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    GithubUserName = "Umer-Iftikhar",
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, defaultPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Setting rule for generating fake users using Bogus
            var faker = new Faker<ApplicationUser>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.GithubUserName, f => f.Internet.UserName())
                .RuleFor(u => u.CreatedDate, f => f.Date.Past(2))
                .RuleFor(u => u.EmailConfirmed, true);

            for (int i = 0; i < 10; i++)
            {
                var devUser = faker.Generate();
                var result = await userManager.CreateAsync(devUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(devUser, "GameDev");
                    devIds.Add(devUser.Id);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                var playerUser = faker.Generate();
                var result = await userManager.CreateAsync(playerUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(playerUser, "Player");
                    playerIds.Add(playerUser.Id);
                }
            }
            return (devIds, playerIds);
        }

        public static async Task<List<Game>> SeedGamesAsync(AppDbContext context, List<string> devIds, List<Genre> genres, List<Engine> engines)
        {
            var games = new List<Game>();
            if (await context.Games.AnyAsync())
            {
                return await context.Games.ToListAsync();
            }
            var faker = new Faker<Game>()
                .RuleFor(g => g.Title, f => f.Commerce.ProductName())
                .RuleFor(g => g.Description, f => f.Lorem.Paragraph())
                .RuleFor(g => g.Price, f => f.Random.Decimal(5, 60))
                .RuleFor(g => g.ReleaseDate, f => f.Date.Past(1))
                .RuleFor(g => g.CoverImagePath, _ => "https://via.placeholder.com/800x600.png?text=Game+Cover")
                .RuleFor(g => g.DownloadLink, f => "https://example.com/download")
                .RuleFor(g => g.IsFeatured, f => f.Random.Bool(0.2f))
                .RuleFor(g => g.CreatedDate, _ => DateTime.UtcNow)
                .RuleFor(g => g.GenreId, f => f.PickRandom(genres).Id)
                .RuleFor(g => g.EngineId, f => f.PickRandom(engines).Id)
                .RuleFor(g => g.DeveloperId, f => f.PickRandom(devIds));

            games = faker.Generate(50);
            await context.Games.AddRangeAsync(games);
            await context.SaveChangesAsync();
            return games;
        }

        public static async Task SeedGamePlatformsAsync(AppDbContext context, List<Game> games, List<Platform> platforms)
        {
            var gamePlatforms = new List<GamePlatform>();
            if (await context.GamePlatforms.AnyAsync())
            {
                return;
            }

            var faker = new Faker();
            foreach (var game in games)
            {
                var selectedPlatforms = faker.PickRandom(platforms, faker.Random.Int(1, 3)); // unique platform pick karo (1 to 3 total per game)
                foreach (var platform in selectedPlatforms)
                {
                    gamePlatforms.Add(new GamePlatform
                    {
                        GameId = game.Id,
                        PlatformId = platform.Id
                    });
                }
            }
            await context.GamePlatforms.AddRangeAsync(gamePlatforms);
            await context.SaveChangesAsync();
        }

        public static async Task SeedGameTagsAsync(AppDbContext context, List<Tag> tags, List<Game> games)
        {
            var gameTags = new List<GameTag>();
            if (await context.GameTags.AnyAsync())
            {
                return;
            }

            var faker = new Faker();
            foreach (var game in games)
            {
                var selectedTags = faker.PickRandom(tags, faker.Random.Int(1, 5)); 
                foreach (var tag in selectedTags)
                {
                    gameTags.Add(new GameTag
                    {
                        GameId = game.Id,
                        TagId = tag.Id
                    });
                }
            }
            await context.GameTags.AddRangeAsync(gameTags);
            await context.SaveChangesAsync();
        }

        public static async Task SeedReviewsAsync(AppDbContext context, List<Game> games, List<string> playerIds)
        {
            var reviews = new List<Review>();
            if (await context.Reviews.AnyAsync())
            {
                return;
            }
            var faker = new Faker();
            foreach (var game in games)
            {
                int reviewCount = faker.Random.Int(1, 5); 
                                                          
                var selectedReviewers = faker.PickRandom(playerIds, reviewCount); // 2. The Constraint Guard: Pick unique users for THIS specific game
                foreach (var playerId in selectedReviewers)
                {
                    reviews.Add(new Review
                    {
                        GameId = game.Id,
                        UserId = playerId,
                        Rating = faker.Random.Int(1, 5),
                        Comment = faker.Rant.Review("game"),
                        ReviewDate = faker.Date.Recent(30)
                    });
                }
            }
            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }
        public static async Task SeedWishlistsAsync(AppDbContext context, List<Game> games, List<string> playerIds)
        {
            var wishlists = new List<Wishlist>();
            if (await context.Wishlists.AnyAsync())
            {
                return;
            }
            var faker = new Faker();
            foreach (var playerId in playerIds)
            {
                int wishlistCount = faker.Random.Int(1, 10); // Each player can have 1-10 games in their wishlist
                var selectedGames = faker.PickRandom(games, wishlistCount); // Pick unique games for THIS specific player's wishlist
                foreach (var game in selectedGames)
                {
                    wishlists.Add(new Wishlist
                    {
                        UserId = playerId,
                        GameId = game.Id,
                        CreatedDate = faker.Date.Recent(30)
                    });
                }
            }
            await context.Wishlists.AddRangeAsync(wishlists);
            await context.SaveChangesAsync();
        }

        public static async Task SeedDownloadHistoriesAsync(AppDbContext context, List<Game> games, List<string> playerIds)
        {
            // Guard Clause
            if (await context.DownloadHistories.AnyAsync()) return;

            var downloads = new List<DownloadHistory>();
            var faker = new Faker();

            foreach (var game in games)
            {
                int downloadCount = Math.Min(faker.Random.Int(5, 12), playerIds.Count); // 1. Each game gets downloaded by 5-12 players (but not more than total players available)
                var selectedDownloaders = faker.PickRandom(playerIds, downloadCount);

                foreach (var playerId in selectedDownloaders)
                {
                    downloads.Add(new DownloadHistory
                    {
                        GameId = game.Id,
                        UserId = playerId,
                        DownloadDate = faker.Date.Recent(60)
                    });
                }
            }

            await context.DownloadHistories.AddRangeAsync(downloads);
            await context.SaveChangesAsync();
        }

        public static async Task SeedScreenshotsAsync(AppDbContext context, List<Game> games)
        {
            if (await context.Screenshots.AnyAsync()) return;

            var screenshots = new List<Screenshot>();
            var faker = new Faker();

            foreach (var game in games)
            {
                // 1. Determine how many screenshots for this specific game
                int screenshotCount = faker.Random.Int(1, 5);

                for (int i = 0; i < screenshotCount; i++)
                {
                    screenshots.Add(new Screenshot
                    {
                        GameId = game.Id,
                        // Using Picsum for varied, high-quality placeholder game art
                        ImagePath = $"https://picsum.photos/seed/{faker.Random.Guid()}/1280/720"
                    });
                }
            }

            await context.Screenshots.AddRangeAsync(screenshots);
            await context.SaveChangesAsync();
        }

    }
}
