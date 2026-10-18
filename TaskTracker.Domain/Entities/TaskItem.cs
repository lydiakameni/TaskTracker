namespace TaskTracker.Domain.Entities;

public class TaskItem: AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    
    
    public int UserId { get; set; }
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