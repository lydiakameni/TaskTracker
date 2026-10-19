using MediatR;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Auth.Commands.Revoke;

/// <summary>
/// Révoque un Refresh Token lors de la déconnexion, pour qu'il ne puisse plus
/// jamais être utilisé, même s'il a été intercepté.
/// </summary>
/// <returns>
/// <c>true</c> si le token a été révoqué ; <c>false</c> s'il était inconnu ou déjà révoqué.
/// </returns>
public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public RevokeTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var storedRefreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);

        // Token inconnu ou déjà révoqué : rien à faire
        if (storedRefreshToken is null || storedRefreshToken.IsRevoked)
            return false;

        storedRefreshToken.IsRevoked = true;
        await _userRepository.UpdateRefreshTokenAsync(storedRefreshToken);

        return true;
    }
}