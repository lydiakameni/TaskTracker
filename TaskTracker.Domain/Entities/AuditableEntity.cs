namespace TaskTracker.Domain.Entities;

/// <summary>
/// Classe de base des entités datées et supprimables logiquement (soft delete).
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>Suppression logique : l'entité est masquée mais conservée en base.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Date de création (UTC).</summary>
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date de dernière modification (UTC).</summary>
    public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;
}