using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.CreateTask;

// La Commande contient les données de création ET l'identité extraite du JWT
public record CreateTaskCommand(
    string Title, 
    int AdminUserId, 
    int? TargetUserId = null // Facultatif : si l'Admin crée la tâche pour un membre précis
) : IRequest<TaskResponseDto>;


public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponseDto>
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponseDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // La tâche appartient soit à l'utilisateur cible désigné, soit à l'admin lui-même
        int assignedUserId = request.TargetUserId ?? request.AdminUserId;

        var task = new TaskItem
        {
            Title = request.Title,
            UserId = assignedUserId,
            IsCompleted = false,
            CreationDate = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId
        };
    }
}