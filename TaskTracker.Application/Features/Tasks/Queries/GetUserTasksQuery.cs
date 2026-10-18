using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Queries;

/// <summary>
/// Requête de récupération des tâches assignées à un utilisateur.
/// </summary>
/// <param name="UserId">Identifiant de l'utilisateur, extrait du JWT.</param>
public record GetUserTasksQuery(int UserId) : IRequest<IEnumerable<TaskResponseDto>>;

/// <summary>
/// Renvoie les tâches non supprimées de l'utilisateur.
/// </summary>
public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, IEnumerable<TaskResponseDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetUserTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponseDto>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllByUserIdAsync(request.UserId);
        return tasks.Select(t => t.ToDto());
    }
}