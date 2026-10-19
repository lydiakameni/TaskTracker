using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskTracker.Application.Behaviors;

namespace TaskTracker.Application;

/// <summary>
/// Enregistrement des services de la couche Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            // Enregistre tous les handlers (commandes et requêtes) de cette couche
            cfg.RegisterServicesFromAssembly(assembly);

            // Chaque requête passe par la validation avant d'atteindre son handler
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Enregistre automatiquement tous les validateurs FluentValidation de cette couche
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}