namespace TaskTracker.Domain.Entities;

/// <summary>
/// Jeton de renouvellement de session, stocké en base pour pouvoir être révoqué.
/// </summary>
/// <remarks>
/// Contrairement à l'Access Token (JWT), ce n'est qu'une chaîne aléatoire :
/// sa validité est vérifiée en base à chaque utilisation.
/// </remarks>
public class RefreshToken
{
    /// <summary>Identifiant unique.</summary>
    public int Id { get; set; }

    /// <summary>Valeur aléatoire du token (32 octets encodés en Base64).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Date d'expiration (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Indique si le token a été révoqué (rotation ou déconnexion).</summary>
    public bool IsRevoked { get; set; }

    /// <summary>Identifiant du propriétaire du token.</summary>
    public int UserId { get; set; }

    /// <summary>Propriétaire du token.</summary>
    public User? User { get; set; }
}