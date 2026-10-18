using Moq;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.Commands.Register;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Constants;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests unitaires du handler d'inscription.
/// Les dépendances (repository, hachage, JWT) sont simulées avec Moq :
/// aucun accès à la base de données n'est nécessaire.
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        // xUnit recrée la classe pour chaque test : chaque test part donc de mocks neufs.
        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object);
    }

    [Fact]
    public async Task Handle_QuandLeNomEstDejaPris_LanceConflictException()
    {
        // Arrange : le repository indique que "lydia" existe déjà
        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync("lydia"))
            .ReturnsAsync(new User { Username = "lydia" });

        var command = new RegisterCommand("lydia", "MotDePasse1");

        // Act + Assert : l'appel doit lancer une ConflictException
        await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Aucun utilisateur ne doit avoir été enregistré
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandLeNomEstLibre_CreeLeCompteEtRetourneLesJetons()
    {
        // Arrange : le nom est libre (GetByUsernameAsync renvoie null par défaut)
        _passwordHasherMock
            .Setup(h => h.Hash("MotDePasse1"))
            .Returns("hash-bcrypt");

        _jwtProviderMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token");

        _jwtProviderMock
            .Setup(j => j.GenerateRefreshToken(It.IsAny<int>()))
            .Returns(new RefreshToken { Token = "refresh-token" });

        var command = new RegisterCommand("lydia", "MotDePasse1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert : les jetons sont bien renvoyés
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        // L'utilisateur est enregistré avec le rôle User et le mot de passe haché
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == "lydia" &&
            u.Role == Roles.User &&
            u.PasswordHash == "hash-bcrypt")), Times.Once);
    }
}