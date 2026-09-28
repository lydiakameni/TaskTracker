namespace TaskTracker.Domain.Entities;

public class User : AuditableEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";

    // Propriétés de navigation
    public List<TaskItem> TaskItems { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}