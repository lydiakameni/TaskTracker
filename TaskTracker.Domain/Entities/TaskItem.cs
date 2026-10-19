namespace TaskTracker.Domain.Entities;

/// <summary>
/// Tâche assignée à un utilisateur.
/// </summary>
public class TaskItem : AuditableEntity
{
    /// <summary>Identifiant unique.</summary>
    public int Id { get; set; }

    /// <summary>Intitulé de la tâche.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Indique si la tâche est terminée.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Identifiant de l'utilisateur à qui la tâche est assignée.</summary>
    public int UserId { get; set; }

    /// <summary>Utilisateur à qui la tâche est assignée.</summary>
    public User? User { get; set; }

    /// <summary>
    /// Marque la tâche comme terminée. Sans effet si elle l'est déjà (opération idempotente).
    /// </summary>
    /// <remarks>
    /// La règle métier vit dans l'entité elle-même : aucun autre code n'a besoin
    /// de savoir comment on « termine » une tâche.
    /// </remarks>
    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }
}