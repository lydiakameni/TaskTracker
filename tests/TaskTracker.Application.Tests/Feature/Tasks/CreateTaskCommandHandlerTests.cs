using Moq;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Tasks.Commands.CreateTask;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Tasks;

/// <summary>
/// Tests unitaires de la création de tâche par un administrateur.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    private const int AdminId = 1;

    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_SansUtilisateurCible_AssigneLaTacheALAdminEtNettoieLeTitre()
    {
        // Arrange : titre avec des espaces en trop
        var command = new CreateTaskCommand("  Préparer la démo  ", AdminId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Préparer la démo", result.Title);
        Assert.Equal(AdminId, result.UserId);
        Assert.False(result.IsCompleted);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AvecUnUtilisateurCibleExistant_AssigneLaTacheACetUtilisateur()
    {
        // Arrange : l'utilisateur 3 existe
        _userRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new User { Id = 3 });
        var command = new CreateTaskCommand("Rédiger le rapport", AdminId, TargetUserId: 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.UserId);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.UserId == 3)), Times.Once);
    }

    [Fact]
    public async Task Handle_AvecUnUtilisateurCibleInexistant_LanceNotFoundExceptionSansRienEnregistrer()
    {
        // Arrange : l'utilisateur 999 n'existe pas (le mock renvoie null)
        var command = new CreateTaskCommand("Tâche orpheline", AdminId, TargetUserId: 999);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
    }
}