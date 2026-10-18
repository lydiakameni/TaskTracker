using Moq;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.Commands.Refresh;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests unitaires du renouvellement de session (rotation des Refresh Tokens).
/// </summary>
public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_userRepositoryMock.Object, _jwtProviderMock.Object);
    }

    /// <summary>
    /// Crée un utilisateur qui possède un Refresh Token "ancien-token",
    /// et configure le repository pour le retrouver à partir de ce token.
    /// </summary>
    private (User User, RefreshToken Token) ArrangeUserWithToken(bool isRevoked = false, DateTime? expiresAt = null)
    {
        var token = new RefreshToken
        {
            Token = "ancien-token",
            UserId = 1,
            IsRevoked = isRevoked,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7)
        };

        var user = new User { Id = 1, Username = "lydia" };
        user.RefreshTokens.Add(token);

        _userRepositoryMock.Setup(r => r.GetByRefreshTokenAsync("ancien-token")).ReturnsAsync(user);

        return (user, token);
    }

    [Fact]
    public async Task Handle_AvecUnTokenValide_RevoqueLAncienEtRetourneDeNouveauxJetons()
    {
        // Arrange
        var (user, oldToken) = ArrangeUserWithToken();
        _jwtProviderMock.Setup(j => j.GenerateAccessToken(user)).Returns("nouvel-access-token");
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken(1))
            .Returns(new RefreshToken { Token = "nouveau-token", UserId = 1, ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(new RefreshTokenCommand("ancien-token"), CancellationToken.None);

        // Assert : nouveaux jetons renvoyés
        Assert.Equal("nouvel-access-token", result.AccessToken);
        Assert.Equal("nouveau-token", result.RefreshToken);

        // Rotation : l'ancien token est révoqué, le nouveau est ajouté, et tout est sauvegardé
        Assert.True(oldToken.IsRevoked);
        Assert.Contains(user.RefreshTokens, t => t.Token == "nouveau-token");
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_AvecUnTokenInconnu_LanceUnauthorizedException()
    {
        // Arrange : aucun utilisateur ne possède ce token (le mock renvoie null)

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new RefreshTokenCommand("token-inconnu"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AvecUnTokenRevoque_LanceUnauthorizedException()
    {
        // Arrange
        ArrangeUserWithToken(isRevoked: true);

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new RefreshTokenCommand("ancien-token"), CancellationToken.None));

        // Aucun nouveau jeton ne doit être émis
        _jwtProviderMock.Verify(j => j.GenerateRefreshToken(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AvecUnTokenExpire_LanceUnauthorizedException()
    {
        // Arrange : le token a expiré hier
        ArrangeUserWithToken(expiresAt: DateTime.UtcNow.AddDays(-1));

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new RefreshTokenCommand("ancien-token"), CancellationToken.None));

        _jwtProviderMock.Verify(j => j.GenerateRefreshToken(It.IsAny<int>()), Times.Never);
    }
}