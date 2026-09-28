using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // Le constructeur reçoit les options de connexion (la chaîne de connexion SQL)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // On déclare nos tables. Entity Framework va créer les tables avec ces noms.
    public DbSet<User> Users { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // On peut utiliser cette méthode pour configurer des règles spécifiques
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Exemple : S'assurer que le Username est unique au niveau de la base de données
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}