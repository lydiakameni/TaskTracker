using TaskTracker.Application.Exceptions;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Features.Tasks;

/// <summary>
/// Règle d'accès commune à toutes les opérations sur une tâche précise.
/// </summary>
public static class TaskAccess
{
    /// <summary>
    /// Charge une tâche si l'utilisateur en est le propriétaire ou s'il est administrateur.
    /// </summary>
    /// <remarks>
    /// Une tâche appartenant à un autre utilisateur est signalée comme introuvable (404)
    /// plutôt qu'interdite (403) : on ne révèle pas son existence, ce qui empêche
    /// de découvrir les tâches des autres en testant les identifiants un par un.
    /// </remarks>
    /// <exception cref="NotFoundException">Tâche inexistante, supprimée ou inaccessible.</exception>
    public static async Task<TaskItem> GetAccessibleTaskAsync(
        this ITaskRepository repository, int taskId, int userId, bool isAdmin)
    {
        var task = await repository.GetByIdAsync(taskId);

        if (task is null || (task.UserId != userId && !isAdmin))
            throw new NotFoundException($"Aucune tâche avec l'identifiant {taskId}.");

        return task;
    }
}