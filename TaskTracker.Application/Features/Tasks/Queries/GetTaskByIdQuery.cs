using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Queries;

/// <summary>
/// Requête de consultation d'une tâche précise.
/// </summary>
/// <param name="TaskId">Identifiant de la tâche.</param>
/// <param name="UserId">Identifiant de l'utilisateur connecté.</param>
/// <param name="IsAdmin">Indique si l'utilisateur connecté est administrateur.</param>
public record GetTaskByIdQuery(int TaskId, int UserId, bool IsAdmin) : IRequest<TaskResponseDto>;

/// <summary>
/// Renvoie la tâche si l'utilisateur y a accès (propriétaire ou administrateur).
/// </summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskResponseDto>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponseDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetAccessibleTaskAsync(request.TaskId, request.UserId, request.IsAdmin);
        return task.ToDto();
    }
}