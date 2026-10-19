using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.Commands.Refresh;
using TaskTracker.Application.Features.Auth.Commands.Register;
using TaskTracker.Application.Features.Auth.Commands.Revoke;
using TaskTracker.Application.Features.Auth.DTOs;
using TaskTracker.Application.Features.Auth.Queries;

namespace TaskTracker.API.Controllers;

/// <summary>
/// Authentification : inscription, connexion, renouvellement du token et déconnexion.
/// </summary>
/// <remarks>
/// L'Access Token (valable 15 minutes) est renvoyé dans le corps de la réponse.
/// Le Refresh Token (valable 7 jours) est stocké dans un cookie HttpOnly,
/// inaccessible au JavaScript du navigateur, ce qui le protège des attaques XSS.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Inscrit un nouvel utilisateur avec le rôle "User".
    /// </summary>
    /// <response code="200">Inscription réussie : Access Token dans le corps, Refresh Token en cookie HttpOnly.</response>
    /// <response code="400">Données invalides (nom d'utilisateur ou mot de passe non conforme).</response>
    /// <response code="409">Ce nom d'utilisateur est déjà utilisé.</response>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AuthResponse(result.AccessToken));
    }

    /// <summary>
    /// Connecte un utilisateur existant.
    /// </summary>
    /// <response code="200">Connexion réussie : Access Token dans le corps, Refresh Token en cookie HttpOnly.</response>
    /// <response code="400">Nom d'utilisateur ou mot de passe manquant.</response>
    /// <response code="401">Nom d'utilisateur ou mot de passe incorrect.</response>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        var result = await _mediator.Send(query);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AuthResponse(result.AccessToken));
    }

    /// <summary>
    /// Renouvelle l'Access Token à partir du Refresh Token contenu dans le cookie.
    /// </summary>
    /// <remarks>
    /// Rotation : l'ancien Refresh Token est révoqué et un nouveau est écrit dans le cookie.
    /// </remarks>
    /// <response code="200">Nouvel Access Token dans le corps, nouveau Refresh Token en cookie.</response>
    /// <response code="401">Refresh Token absent, invalide, expiré ou révoqué.</response>
    [HttpPost("refresh_token")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName]
            ?? throw new UnauthorizedException("Refresh Token manquant dans les cookies.");

        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AuthResponse(result.AccessToken));
    }

    /// <summary>
    /// Déconnecte l'utilisateur : révoque le Refresh Token en base et supprime le cookie.
    /// </summary>
    /// <response code="200">Déconnexion effectuée.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrEmpty(refreshToken))
            await _mediator.Send(new RevokeTokenCommand(refreshToken));

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(new { message = "Déconnexion réussie." });
    }

    /// <summary>
    /// Écrit le Refresh Token dans un cookie sécurisé.
    /// </summary>
    /// <remarks>
    /// HttpOnly : illisible par JavaScript (protection XSS).
    /// Secure : envoyé uniquement en HTTPS.
    /// SameSite=Strict : jamais envoyé depuis un autre site (protection CSRF).
    /// </remarks>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }
}