using TaskTracker.Domain.Constants;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Utilisateur de l'application.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Identifiant unique.</summary>
    public int Id { get; set; }

    /// <summary>Nom de connexion, unique en base.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Mot de passe haché avec BCrypt. Le mot de passe n'est jamais stocké en clair.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Rôle applicatif (voir <see cref="Roles"/>).</summary>
    public string Role { get; set; } = Roles.User;

    /// <summary>Tâches assignées à l'utilisateur.</summary>
    public List<TaskItem> TaskItems { get; set; } = new();

    /// <summary>Historique des Refresh Tokens émis (actifs, expirés ou révoqués).</summary>
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}