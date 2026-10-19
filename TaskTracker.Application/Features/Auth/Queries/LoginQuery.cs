using MediatR;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Application.Features.Auth.DTOs;

namespace TaskTracker.Application.Features.Auth.Queries;

// La Requête demande les jetons d'accès en fournissant les identifiants
public record LoginQuery(string Username, string Password) : IRequest<AuthenticationResult>;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginQueryHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthenticationResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        
        // On fusionne la vérification pour ne pas donner d'indice aux pirates (ex: "utilisateur non trouvé" vs "mot de passe faux")
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Nom d'utilisateur ou mot de passe incorrect.");
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _userRepository.UpdateAsync(user);

        return new AuthenticationResult(accessToken, refreshToken.Token);
    }
}