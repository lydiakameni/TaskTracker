using MediatR;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Auth.Commands.Revoke;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public RevokeTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Chercher le token en BDD
        var storedRefreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);

        // Si le token n'existe pas ou est déjà expiré / révoqué
        if (storedRefreshToken == null || !storedRefreshToken.IsRevoked)
        {
            return false;
        }

        // 2. Marquer le token comme révoqué
        storedRefreshToken.IsRevoked = true;
        storedRefreshToken.ExpiresAt = DateTime.UtcNow;

        // 3. Sauvegarder en BDD
        await _userRepository.UpdateRefreshTokenAsync(storedRefreshToken);

        return true;
    }
}