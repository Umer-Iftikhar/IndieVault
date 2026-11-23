using IndieVault.Data;
using IndieVault.Extensions;
using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using IndieVault.Services.Implementations.ExternalApis;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddControllersWithViews();

    // Register services
    builder.Services.AddScoped<IGameService, GameService>();
    builder.Services.AddScoped<IGameBrowseService, GameBrowseService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IWishlistService, WishlistService>();
    builder.Services.AddScoped<IDownloadService, DownloadService>();
    builder.Services.AddScoped<IAdminService, AdminService>();

    builder.Services.AddHttpClient<IGitHubService, GitHubService>(httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://api.github.com/");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "IndieVault");
    });

    // Register repositories
    builder.Services.AddScoped<IGenreRepository, GenreRepository>();
    builder.Services.AddScoped<ITagRepository, TagRepository>();
    builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
    builder.Services.AddScoped<IEngineRepository, EngineRepository>();
    builder.Services.AddScoped<IGameRepository, GameRepository>();
    builder.Services.AddScoped<IScreenshotRepository, ScreenshotRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
    builder.Services.AddScoped<IDownloadRepository, DownloadRepository>();
    builder.Services.AddScoped<IGameBrowseRepository, GameBrowseRepository>();
    builder.Services.AddScoped<IAdminStatisticsRepository, AdminStatisticsRepository>();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));

    builder.Services.AddDbContext<AppDbContext>
    (
        options => options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
    );

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 1;
        options.User.RequireUniqueEmail = true;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseGlobalExceptionMiddleware();
        app.UseHsts();
    }

    // Re-execute rather than redirect: a missing page returns a real 404, not 302 then 200
    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    if (app.Environment.IsDevelopment())
    {
        await DatabaseSeeder.SeedAsync(app.Services);
    }

    app.UseRouting();

    app.UseAuthentication();

    // After authentication so the logged-in user name is available in the logs
    app.UseRequestLoggingMiddleware();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { } // lets the test projects reference Program
