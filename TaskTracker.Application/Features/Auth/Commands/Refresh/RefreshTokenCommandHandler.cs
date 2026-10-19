using MediatR;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Auth.Commands.Refresh;

/// <summary>
/// Renouvelle l'Access Token à partir d'un Refresh Token valide, avec rotation :
/// l'ancien Refresh Token est révoqué et un nouveau est émis à chaque appel.
/// </summary>
/// <remarks>
/// La rotation limite l'impact d'un vol de Refresh Token : un token volé
/// ne peut être utilisé qu'une seule fois.
/// </remarks>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken)
            ?? throw new UnauthorizedException("Refresh Token invalide.");

        var existingToken = user.RefreshTokens.Single(rt => rt.Token == request.RefreshToken);

        if (existingToken.IsRevoked)
            throw new UnauthorizedException("Ce Refresh Token a été révoqué. Veuillez vous reconnecter.");

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Ce Refresh Token a expiré. Veuillez vous reconnecter.");

        // Rotation : l'ancien token devient inutilisable
        existingToken.IsRevoked = true;

        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateAsync(user); // Enregistre la révocation et le nouveau token

        return new AuthenticationResult(newAccessToken, newRefreshToken.Token);
    }
}