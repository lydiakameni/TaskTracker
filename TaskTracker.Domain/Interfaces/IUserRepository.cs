using TaskTracker.Domain.Entities;

namespace TaskTracker.Domain.Interfaces;

/// <summary>
/// Accès aux données des utilisateurs et de leurs Refresh Tokens.
/// Les utilisateurs supprimés logiquement sont exclus des lectures.
/// </summary>
public interface IUserRepository
{
    /// <summary>Renvoie un utilisateur par son nom, avec ses Refresh Tokens, ou <c>null</c>.</summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Renvoie un utilisateur par son identifiant, ou <c>null</c>.</summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>Ajoute un utilisateur. Son identifiant est renseigné après l'appel.</summary>
    Task AddAsync(User user);

    /// <summary>Enregistre les modifications d'un utilisateur et de ses Refresh Tokens.</summary>
    Task UpdateAsync(User user);

    /// <summary>Renvoie l'utilisateur propriétaire d'un Refresh Token, avec ses tokens, ou <c>null</c>.</summary>
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    /// <summary>Renvoie un Refresh Token par sa valeur, ou <c>null</c>.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>Enregistre les modifications d'un Refresh Token (ex. : révocation).</summary>
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
}