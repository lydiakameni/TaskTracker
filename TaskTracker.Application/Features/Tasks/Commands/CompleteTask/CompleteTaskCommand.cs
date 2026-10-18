using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.CompleteTask;

/// <summary>
/// Commande permettant de marquer une tâche comme terminée.
/// </summary>
/// <param name="TaskId">Identifiant de la tâche.</param>
/// <param name="UserId">Identifiant de l'utilisateur connecté.</param>
/// <param name="IsAdmin">Indique si l'utilisateur connecté est administrateur.</param>
public record CompleteTaskCommand(int TaskId, int UserId, bool IsAdmin) : IRequest<TaskResponseDto>;

/// <summary>
/// Marque la tâche comme terminée. Autorisé au propriétaire de la tâche ou à un administrateur.
/// L'opération est idempotente : la rappeler sur une tâche déjà terminée ne change rien.
/// </summary>
public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, TaskResponseDto>
{
    private readonly ITaskRepository _taskRepository;

    public CompleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponseDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetAccessibleTaskAsync(request.TaskId, request.UserId, request.IsAdmin);

        task.MarkAsCompleted();
        await _taskRepository.UpdateAsync(task);

        return task.ToDto();
    }
}