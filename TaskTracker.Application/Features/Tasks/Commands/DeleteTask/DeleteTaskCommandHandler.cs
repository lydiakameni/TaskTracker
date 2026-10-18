using MediatR;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Tasks;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Supprime une tâche (soft delete). Autorisé au propriétaire de la tâche ou à un administrateur.
/// </summary>
/// <exception cref="NotFoundException">Tâche inexistante, déjà supprimée ou inaccessible (404).</exception>
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetAccessibleTaskAsync(request.TaskId, request.UserId, request.IsAdmin);
        await _taskRepository.DeleteAsync(task);
    }
}