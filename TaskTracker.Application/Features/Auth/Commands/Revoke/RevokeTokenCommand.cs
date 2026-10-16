using MediatR;

namespace TaskTracker.Application.Features.Auth.Commands.Revoke;

public record RevokeTokenCommand(string RefreshToken) : IRequest<bool>;