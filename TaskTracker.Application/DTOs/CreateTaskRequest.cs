namespace TaskTracker.Application.DTOs;

public record CreateTaskRequest(string Title,int? TargetUserId = null);