using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Commands.CreateTask;
using TaskTracker.Application.Features.Tasks.Commands.DeleteTask;
using TaskTracker.Application.Features.Tasks.Queries;

namespace TaskTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    // Récupère les tâches de l'utilisateur connecté
    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized();

        var tasks = await _mediator.Send(new GetUserTasksQuery(userId));
        return Ok(tasks);
    }

    // Création d'une tâche (Réservé aux Administrateurs)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int adminId))
            return Unauthorized();

        var command = new CreateTaskCommand(request.Title, adminId, request.TargetUserId);
        var createdTask = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetMyTasks), new { id = createdTask.Id }, createdTask);
    }

    // Suppression d'une tâche par son ID (Réservé aux Administrateurs)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized();

        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var success = await _mediator.Send(new DeleteTaskCommand(id, userId, userRole));

        return success ? NoContent() : NotFound(new { message = $"Tâche avec l'ID {id} introuvable." });
    }
}