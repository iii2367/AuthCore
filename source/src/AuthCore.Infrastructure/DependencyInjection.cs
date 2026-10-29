using AuthCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");
        }

        services.AddDbContext<AuthCoreDbContext>(options => options.UseNpgsql(connectionString));

        // Час через вбудований TimeProvider (у тестах підміняється FakeTimeProvider)
        services.AddSingleton(TimeProvider.System);

        // Далі по мірі появи:
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        // services.AddSingleton<ITokenService, JwtTokenService>();
        return services;
    }
}
