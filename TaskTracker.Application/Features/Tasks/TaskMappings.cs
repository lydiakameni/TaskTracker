using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Features.Tasks;

/// <summary>
/// Conversion des entités <see cref="TaskItem"/> en objets de réponse.
/// Centraliser ce mapping garantit que tous les endpoints renvoient
/// exactement le même format.
/// </summary>
public static class TaskMappings
{
    public static TaskResponseDto ToDto(this TaskItem task) =>
        new(task.Id, task.Title, task.IsCompleted, task.UserId, task.CreationDate);
}