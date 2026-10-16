using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Exceptions;

namespace TaskTracker.API.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Une exception est survenue : {Message}", exception.Message);

        // Détermination du code HTTP et du titre selon le type d'exception
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable"), // Exemple : utilisateur non trouvé
            ConflictException => (StatusCodes.Status409Conflict, "Conflit"), // Exemple : nom d'utilisateur déjà utilisé
            UnauthorizedAccessException or UnauthorizedException => (StatusCodes.Status401Unauthorized, "Non autorisé"), // Exemple : accès à une ressource protégée sans authentification
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Clé introuvable"), // Exemple : tentative de suppression d'un élément inexistant
            InvalidOperationException or ArgumentException => (StatusCodes.Status400BadRequest, "Requête invalide"), // Exemple : tentative d'opération non autorisée ou argument invalide
            _ => (StatusCodes.Status500InternalServerError, "Erreur serveur interne") // Pour toutes les autres exceptions non gérées
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Indique que l'exception a été complètement traitée
    }
}