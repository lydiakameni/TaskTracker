using Moq;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.Queries;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests unitaires du handler de connexion.
/// </summary>
public class LoginQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly LoginQueryHandler _handler;

    public LoginQueryHandlerTests()
    {
        _handler = new LoginQueryHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object);
    }

    /// <summary>Crée un utilisateur de test existant en base.</summary>
    private static User CreateUser() => new()
    {
        Id = 1,
        Username = "lydia",
        PasswordHash = "hash-bcrypt"
    };

    [Fact]
    public async Task Handle_AvecDesIdentifiantsValides_RetourneLesJetonsEtEnregistreLeRefreshToken()
    {
        // Arrange
        var user = CreateUser();
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("lydia")).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("MotDePasse1", "hash-bcrypt")).Returns(true);
        _jwtProviderMock.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken(1))
            .Returns(new RefreshToken { Token = "refresh-token", UserId = 1 });

        // Act
        var result = await _handler.Handle(new LoginQuery("lydia", "MotDePasse1"), CancellationToken.None);

        // Assert
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        // Le Refresh Token est rattaché à l'utilisateur et sauvegardé
        Assert.Contains(user.RefreshTokens, t => t.Token == "refresh-token");
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_AvecUnMauvaisMotDePasse_LanceUnauthorizedException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("lydia")).ReturnsAsync(CreateUser());
        _passwordHasherMock.Setup(h => h.Verify("Mauvais1", "hash-bcrypt")).Returns(false);

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginQuery("lydia", "Mauvais1"), CancellationToken.None));

        // Aucun jeton ne doit être généré
        _jwtProviderMock.Verify(j => j.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AvecUnUtilisateurInconnu_LanceUnauthorizedExceptionAvecLeMemeMessage()
    {
        // Arrange : GetByUsernameAsync renvoie null par défaut (utilisateur introuvable)
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("lydia")).ReturnsAsync(CreateUser());
        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var wrongPassword = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginQuery("lydia", "Mauvais1"), CancellationToken.None));

        var unknownUser = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginQuery("inconnu", "MotDePasse1"), CancellationToken.None));

        // Assert : même message dans les deux cas, pour ne donner aucun indice à un attaquant
        Assert.Equal(wrongPassword.Message, unknownUser.Message);
    }
}