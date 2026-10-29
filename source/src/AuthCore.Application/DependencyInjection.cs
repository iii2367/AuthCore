using Microsoft.Extensions.DependencyInjection;

namespace AuthCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Хендлери use case-ів реєструються тут по мірі появи, наприклад:
        // services.AddScoped<LoginHandler>();
        return services;
    }
}
