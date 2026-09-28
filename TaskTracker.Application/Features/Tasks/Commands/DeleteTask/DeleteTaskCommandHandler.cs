using MediatR;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Vérifier si la tâche existe
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            return false; // Tâche introuvable
        }

        // 2. Vérification des droits : L'utilisateur doit être le propriétaire OU un Admin
        if (task.UserId != request.UserId && request.UserRole != "Admin")
        {
            throw new UnauthorizedAccessException("Vous n'avez pas l'autorisation de supprimer cette tâche.");
        }

        // 3. Suppression
        await _taskRepository.DeleteAsync(task);
        return true;
    }
}