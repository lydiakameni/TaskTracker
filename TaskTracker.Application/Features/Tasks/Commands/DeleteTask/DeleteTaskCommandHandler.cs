using MediatR;
using TaskTracker.Application.Exceptions;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Supprime une tâche (soft delete). Autorisé au propriétaire de la tâche ou à un administrateur.
/// </summary>
/// <exception cref="NotFoundException">La tâche n'existe pas ou a déjà été supprimée (404).</exception>
/// <exception cref="UnauthorizedAccessException">L'utilisateur n'a pas le droit de la supprimer (403).</exception>
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId)
            ?? throw new NotFoundException($"Aucune tâche avec l'identifiant {request.TaskId}.");

        if (task.UserId != request.UserId && !request.IsAdmin)
            throw new UnauthorizedAccessException("Vous n'avez pas l'autorisation de supprimer cette tâche.");

        await _taskRepository.DeleteAsync(task);
    }
}