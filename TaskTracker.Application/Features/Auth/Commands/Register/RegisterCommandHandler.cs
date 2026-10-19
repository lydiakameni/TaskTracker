using MediatR;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Constants;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Auth.Commands.Register;

/// <summary>
/// Gère l'inscription : vérifie l'unicité du nom d'utilisateur, hache le mot de passe,
/// crée le compte avec le rôle "User" et génère une paire de jetons (Access + Refresh).
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthenticationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser is not null)
            throw new ConflictException("Ce nom d'utilisateur est déjà pris.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Roles.User // Rôle imposé par le serveur, jamais fourni par le client
        };

        // L'insertion génère l'identifiant de l'utilisateur, nécessaire au Refresh Token
        await _userRepository.AddAsync(user);

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _userRepository.UpdateAsync(user);

        return new AuthenticationResult(accessToken, refreshToken.Token);
    }
}