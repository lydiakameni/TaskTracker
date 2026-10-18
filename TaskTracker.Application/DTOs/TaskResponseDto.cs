namespace TaskTracker.Application.DTOs;

/// <summary>
/// Représentation d'une tâche renvoyée par l'API.
/// </summary>
/// <param name="Id">Identifiant de la tâche.</param>
/// <param name="Title">Intitulé de la tâche.</param>
/// <param name="IsCompleted">Indique si la tâche est terminée.</param>
/// <param name="UserId">Identifiant de l'utilisateur à qui la tâche est assignée.</param>
/// <param name="CreationDate">Date de création (UTC).</param>
public record TaskResponseDto(int Id, string Title, bool IsCompleted, int UserId, DateTime CreationDate);