using AuthCore.Application;
using AuthCore.Infrastructure;
using AuthCore.Infrastructure.Persistence;

var runSeed = args.Contains("--seed", StringComparer.Ordinal);
var runMigrate = args.Contains("--migrate", StringComparer.Ordinal);

var hostArgs = args
    .Where(arg =>
        !string.Equals(arg, "--seed", StringComparison.Ordinal) &&
        !string.Equals(arg, "--migrate", StringComparison.Ordinal))
    .ToArray();

var builder = WebApplication.CreateBuilder(hostArgs);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (runMigrate || runSeed)
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();

        var initializer = scope.ServiceProvider.GetRequiredService<AuthCoreDatabaseInitializer>();

        if (runMigrate)
        {
            Console.WriteLine("Applying database migrations...");
            await initializer.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
        }

        if (runSeed)
        {
            Console.WriteLine("Seeding database...");
            await initializer.SeedAsync();
            Console.WriteLine("Database seeded successfully.");
        }
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Database initialization failed: {exception.Message}");
        Environment.ExitCode = 1;
    }

    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Потрібно для WebApplicationFactory<Program> в інтеграційних тестах
public partial class Program;
