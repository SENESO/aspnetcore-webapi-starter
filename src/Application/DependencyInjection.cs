using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

// One call from Api wires up everything in this layer.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<AuthService>();
        return services;
    }
}
