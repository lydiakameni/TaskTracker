namespace TaskTracker.Domain.Constants;

/// <summary>
/// Rôles applicatifs disponibles.
/// Utiliser ces constantes plutôt que des chaînes en dur évite les fautes de frappe
/// et centralise la définition des rôles.
/// </summary>
public static class Roles
{
    /// <summary>Administrateur : peut créer et supprimer des tâches pour tous les utilisateurs.</summary>
    public const string Admin = "Admin";

    /// <summary>Utilisateur standard : consulte ses propres tâches.</summary>
    public const string User = "User";
}