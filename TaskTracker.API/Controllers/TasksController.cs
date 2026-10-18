using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Tasks.Commands.CreateTask;
using TaskTracker.Application.Features.Tasks.Commands.DeleteTask;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Constants;

namespace TaskTracker.API.Controllers;

/// <summary>
/// Gestion des tâches. Toutes les routes nécessitent un Access Token JWT valide.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Renvoie les tâches assignées à l'utilisateur connecté.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<TaskResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks()
    {
        var tasks = await _mediator.Send(new GetUserTasksQuery(GetCurrentUserId()));
        return Ok(tasks);
    }

    /// <summary>
    /// Crée une tâche, assignée à l'administrateur ou à un utilisateur cible. Réservé aux administrateurs.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<TaskResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand(request.Title, GetCurrentUserId(), request.TargetUserId);
        var createdTask = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetMyTasks), createdTask);
    }

    /// <summary>
    /// Supprime une tâche (suppression logique). Réservé aux administrateurs.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        await _mediator.Send(new DeleteTaskCommand(id, GetCurrentUserId(), User.IsInRole(Roles.Admin)));
        return NoContent();
    }

    /// <summary>
    /// Lit l'identifiant de l'utilisateur connecté depuis le JWT.
    /// </summary>
    /// <exception cref="UnauthorizedException">Le token ne contient pas d'identifiant valide.</exception>
    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedException("Identifiant utilisateur absent du token.");
    }
}