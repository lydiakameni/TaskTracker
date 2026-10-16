using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Exceptions;
using FluentValidation;

namespace TaskTracker.API.Middlewares;

/// <summary>
/// Intercepte toutes les exceptions non gérées et les convertit en réponses
/// standardisées au format ProblemDetails (RFC 7807).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Erreurs métier connues (401, 403, 404, 409) : le message est renvoyé au client.</item>
/// <item>Erreurs inattendues (500) : message générique côté client, détail technique
/// uniquement dans les journaux, pour ne jamais exposer d'informations internes.</item>
/// </list>
/// Chaque réponse contient un <c>traceId</c> permettant de retrouver l'erreur dans les journaux.
/// </remarks>
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
        var (statusCode, title) = exception switch
        {
            ValidationException         => (StatusCodes.Status400BadRequest, "Données invalides"),// Exemple : format JSON invalide ou règles FluentValidation non respectées
            UnauthorizedException       => (StatusCodes.Status401Unauthorized, "Non authentifié"),// Exemple : jeton JWT invalide ou expiré
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Accès refusé"),// Exemple : utilisateur authentifié mais sans le rôle requis
            NotFoundException           => (StatusCodes.Status404NotFound, "Ressource introuvable"),// Exemple : ressource demandée inexistante
            ConflictException           => (StatusCodes.Status409Conflict, "Conflit"),// Exemple : tentative de création d'une ressource déjà existante
            _                           => (StatusCodes.Status500InternalServerError, "Erreur serveur interne")// Exemple : exception inattendue, bug à corriger
        };

        var isServerError = statusCode == StatusCodes.Status500InternalServerError;

        // Une erreur métier est un comportement attendu (Warning) ;
        // une erreur 500 est un bug à corriger (Error, avec la stack trace complète).
        if (isServerError)
        {
            _logger.LogError(exception, "Erreur inattendue sur {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Erreur métier {StatusCode} sur {Method} {Path} : {Message}",
                statusCode, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception switch
                {
                    ValidationException => "Une ou plusieurs données envoyées sont invalides.",
                    _ when isServerError => "Une erreur inattendue est survenue. Veuillez réessayer plus tard.",
                    _ => exception.Message
                }, // Le détail technique n'est jamais exposé côté client pour les erreurs 500, uniquement dans les journaux.
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Pour une erreur de validation, on liste les messages par champ :
        // le frontend peut ainsi afficher chaque erreur sous le bon champ du formulaire.
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true; // L'exception est entièrement traitée
    }
}