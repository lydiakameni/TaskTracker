using MediatR;
using TaskTracker.Application.Features.Auth.DTOs;

namespace TaskTracker.Application.Features.Auth.Commands.Refresh;

// Le client nous envoie l'ancien token pour prouver son identité
public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthenticationResult>;