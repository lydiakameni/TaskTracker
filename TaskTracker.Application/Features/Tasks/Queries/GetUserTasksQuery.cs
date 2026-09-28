using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Queries;

// La Requête (Query) transporte l'information extraite du JWT
public record GetUserTasksQuery(int UserId) : IRequest<IEnumerable<TaskResponseDto>>;

// Le Handler traite la requête
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
        
        return tasks.Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted
        });
    }
}