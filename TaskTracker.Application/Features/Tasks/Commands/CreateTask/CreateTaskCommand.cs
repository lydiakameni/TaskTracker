using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Exceptions;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Commande de création d'une tâche par un administrateur.
/// </summary>
/// <param name="Title">Intitulé de la tâche.</param>
/// <param name="AdminUserId">Identifiant de l'administrateur, extrait du JWT.</param>
/// <param name="TargetUserId">
/// Utilisateur à qui assigner la tâche. Si absent, la tâche est assignée à l'administrateur lui-même.
/// </param>
public record CreateTaskCommand(string Title, int AdminUserId, int? TargetUserId = null)
    : IRequest<TaskResponseDto>;

/// <summary>
/// Crée la tâche après avoir vérifié que l'utilisateur cible existe.
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponseDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<TaskResponseDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Sans cette vérification, une cible inexistante provoquerait une erreur SQL (clé étrangère),
        // donc une réponse 500 au lieu d'un 404 explicite.
        if (request.TargetUserId is int targetUserId)
        {
            _ = await _userRepository.GetByIdAsync(targetUserId)
                ?? throw new NotFoundException($"Aucun utilisateur avec l'identifiant {targetUserId}.");
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            UserId = request.TargetUserId ?? request.AdminUserId,
            IsCompleted = false
        };

        await _taskRepository.AddAsync(task);

        return task.ToDto();
    }
}