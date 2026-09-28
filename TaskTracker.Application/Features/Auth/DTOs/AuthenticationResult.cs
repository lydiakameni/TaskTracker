namespace TaskTracker.Application.Features.Auth.DTOs;

public record AuthenticationResult(string AccessToken, string RefreshToken);