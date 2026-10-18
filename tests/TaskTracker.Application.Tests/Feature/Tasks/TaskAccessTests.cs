using Moq;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Tasks.Commands.CompleteTask;
using TaskTracker.Application.Features.Tasks.Commands.DeleteTask;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Tests.Features.Tasks;

/// <summary>
/// Tests de la règle d'accès « propriétaire ou administrateur »
/// sur la consultation, la complétion et la suppression d'une tâche.
/// </summary>
public class TaskAccessTests
{
    private const int TaskId = 10;
    private const int OwnerId = 1;
    private const int OtherUserId = 2;
    private const int AdminId = 99;

    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly TaskItem _task;

    public TaskAccessTests()
    {
        // Une tâche n°10 appartenant à l'utilisateur 1
        _task = new TaskItem { Id = TaskId, Title = "Tâche de Lydia", UserId = OwnerId };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(TaskId)).ReturnsAsync(_task);
    }

    // ---------- Consultation ----------

    [Theory]
    [InlineData(OwnerId, false)] // le propriétaire
    [InlineData(AdminId, true)]  // un administrateur
    public async Task GetTaskById_ParUnUtilisateurAutorise_RetourneLaTache(int userId, bool isAdmin)
    {
        var handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object);

        var result = await handler.Handle(new GetTaskByIdQuery(TaskId, userId, isAdmin), CancellationToken.None);

        Assert.Equal(TaskId, result.Id);
        Assert.Equal("Tâche de Lydia", result.Title);
    }

    [Fact]
    public async Task GetTaskById_ParUnAutreUtilisateur_LanceNotFoundException()
    {
        // 404 et non 403 : on ne révèle pas l'existence de la tâche d'un autre utilisateur
        var handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTaskByIdQuery(TaskId, OtherUserId, false), CancellationToken.None));
    }

    [Fact]
    public async Task GetTaskById_AvecUneTacheInexistante_LanceNotFoundException()
    {
        var handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTaskByIdQuery(TaskId: 404, OwnerId, false), CancellationToken.None));
    }

    // ---------- Complétion ----------

    [Fact]
    public async Task CompleteTask_ParLeProprietaire_MarqueLaTacheTermineeEtLEnregistre()
    {
        var handler = new CompleteTaskCommandHandler(_taskRepositoryMock.Object);

        var result = await handler.Handle(new CompleteTaskCommand(TaskId, OwnerId, false), CancellationToken.None);

        Assert.True(result.IsCompleted);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(_task), Times.Once);
    }

    [Fact]
    public async Task CompleteTask_ParUnAutreUtilisateur_LanceNotFoundExceptionSansModifier()
    {
        var handler = new CompleteTaskCommandHandler(_taskRepositoryMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new CompleteTaskCommand(TaskId, OtherUserId, false), CancellationToken.None));

        Assert.False(_task.IsCompleted);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    // ---------- Suppression ----------

    [Fact]
    public async Task DeleteTask_ParUnAdministrateur_SupprimeLaTache()
    {
        var handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object);

        await handler.Handle(new DeleteTaskCommand(TaskId, AdminId, true), CancellationToken.None);

        _taskRepositoryMock.Verify(r => r.DeleteAsync(_task), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_ParUnAutreUtilisateur_LanceNotFoundExceptionSansSupprimer()
    {
        var handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteTaskCommand(TaskId, OtherUserId, false), CancellationToken.None));

        _taskRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }
}