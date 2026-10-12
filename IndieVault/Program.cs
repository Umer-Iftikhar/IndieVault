using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger(); // logs startup errors before the host exists

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseGlobalExceptionMiddleware();
        app.UseHsts();
    }

    app.UseRequestLoggingMiddleware();

    app.MapGet("/", () => "IndieVault is up.");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException) // dotnet ef throws this on purpose
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
