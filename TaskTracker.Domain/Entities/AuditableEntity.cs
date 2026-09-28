namespace TaskTracker.Domain.Entities;

public abstract class AuditableEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;
}