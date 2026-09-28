using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TaskTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Enregistre MediatR et lui dit de chercher les Handlers dans l'assembly courant
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}