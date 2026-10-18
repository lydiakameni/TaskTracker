using MediatR;

namespace TaskTracker.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Commande de suppression (logique) d'une tâche.
/// </summary>
/// <param name="TaskId">Identifiant de la tâche à supprimer.</param>
/// <param name="UserId">Identifiant de l'utilisateur connecté.</param>
/// <param name="IsAdmin">Indique si l'utilisateur connecté est administrateur.</param>
public record DeleteTaskCommand(int TaskId, int UserId, bool IsAdmin) : IRequest;