using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.API.IntegrationTests;

/// <summary>
/// Démarre l'API complète en mémoire pour les tests d'intégration.
/// </summary>
/// <remarks>
/// SQL Server est remplacé par une base EF Core InMemory propre à chaque fabrique :
/// les tests n'accèdent jamais à la vraie base et peuvent tourner sur n'importe quelle machine.
/// </remarks>
public class TaskTrackerApiFactory : WebApplicationFactory<Program>
{
    /// <summary>Clé JWT réservée aux tests (au moins 32 caractères). Jamais utilisée en production.</summary>
    public const string TestJwtSecretKey = "cle-jwt-reservee-aux-tests-uniquement-0123456789";

    private readonly string _databaseName = $"TaskTrackerTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environnement dédié : les user-secrets (Development) ne sont pas chargés
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:SecretKey", TestJwtSecretKey);

        builder.ConfigureServices(services =>
        {
            // On retire la configuration SQL Server enregistrée par AddInfrastructure...
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            // ...et on la remplace par une base en mémoire
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}