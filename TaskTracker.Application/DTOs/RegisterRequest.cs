// Gestion de registre de connexion


namespace TaskTracker.Application.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}