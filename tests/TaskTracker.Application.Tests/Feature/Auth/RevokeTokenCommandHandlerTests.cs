using Moq;
using TaskTracker.Application.Features.Auth.Commands.Revoke;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests unitaires de la révocation d'un Refresh Token (déconnexion).
/// </summary>
public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _handler = new RevokeTokenCommandHandler(_userRepositoryMock.Object);
    }

    /// <summary>
    /// Non-régression : une ancienne version ne révoquait jamais un token actif
    /// à cause d'une condition inversée (<c>!IsRevoked</c>).
    /// </summary>
    [Fact]
    public async Task Handle_AvecUnTokenActif_LeRevoqueEtRetourneTrue()
    {
        // Arrange
        var token = new RefreshToken { Token = "token-actif", IsRevoked = false };
        _userRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token-actif")).ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(new RevokeTokenCommand("token-actif"), CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(token.IsRevoked);
        _userRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_AvecUnTokenInconnu_RetourneFalse()
    {
        // Act
        var result = await _handler.Handle(new RevokeTokenCommand("token-inconnu"), CancellationToken.None);

        // Assert
        Assert.False(result);
        _userRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AvecUnTokenDejaRevoque_RetourneFalseSansRienModifier()
    {
        // Arrange
        var token = new RefreshToken { Token = "token-revoque", IsRevoked = true };
        _userRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token-revoque")).ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(new RevokeTokenCommand("token-revoque"), CancellationToken.None);

        // Assert
        Assert.False(result);
        _userRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
}