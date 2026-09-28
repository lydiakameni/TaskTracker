using MediatR;

namespace TaskTracker.Application.Features.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(int TaskId, int UserId, string UserRole) : IRequest<bool>;