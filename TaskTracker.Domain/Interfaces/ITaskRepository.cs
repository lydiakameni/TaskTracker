using TaskTracker.Domain.Entities;

namespace TaskTracker.Domain.Interfaces;

/// <summary>
/// Accès aux données des tâches. Les tâches supprimées logiquement sont toujours exclues des lectures.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Renvoie les tâches actives assignées à un utilisateur.</summary>
    Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId);

    /// <summary>Renvoie une tâche active par son identifiant, ou <c>null</c> si elle n'existe pas.</summary>
    Task<TaskItem?> GetByIdAsync(int id);

    /// <summary>Ajoute une tâche. Son identifiant est renseigné après l'appel.</summary>
    Task AddAsync(TaskItem task);

    /// <summary>Enregistre les modifications d'une tâche.</summary>
    Task UpdateAsync(TaskItem task);

    /// <summary>Supprime logiquement une tâche (soft delete).</summary>
    Task DeleteAsync(TaskItem task);
}