using MediatR;
using TaskTracker.Application.Features.Auth.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Auth.Commands.Refresh;

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
        // 1. On cherche l'utilisateur lié à ce Refresh Token
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null)
            throw new Exception("Token invalide.");

        // 2. On récupère l'objet token spécifique depuis la liste de l'utilisateur
        var existingToken = user.RefreshTokens.First(rt => rt.Token == request.RefreshToken);

        // 3. VÉRIFICATIONS DE SÉCURITÉ
        if (existingToken.IsRevoked)
            throw new Exception("Ce token a été révoqué. Veuillez vous reconnecter.");

        if (existingToken.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Le token a expiré. Veuillez vous reconnecter.");

        // 4. ROTATION DES TOKENS (Bonne pratique de sécurité)
        // On révoque l'ancien token pour qu'il ne puisse plus jamais être utilisé
        existingToken.IsRevoked = true;

        // 5. Génération des nouveaux tokens
        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateAsync(user); // Sauvegarde la révocation et l'ajout en BDD

        return new AuthenticationResult(newAccessToken, newRefreshToken.Token);
    }
}