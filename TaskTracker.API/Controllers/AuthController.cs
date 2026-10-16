using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Features.Auth.DTOs;
using TaskTracker.Application.Exceptions;
using TaskTracker.Application.Features.Auth.Commands.Register;
using TaskTracker.Application.Features.Auth.Commands.Refresh;
using TaskTracker.Application.Features.Auth.Commands.Revoke;
using TaskTracker.Application.Features.Auth.Queries;

namespace TaskTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Inscrit un nouvel utilisateur avec le rôle "User".
    /// </summary>
    /// <response code="200">Inscription réussie : Access Token dans le corps, Refresh Token en cookie HttpOnly.</response>
    /// <response code="409">Ce nom d'utilisateur est déjà utilisé.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AuthResponse(result.AccessToken));
    }

    // Connexion : renvoie l'AccessToken et écrit le RefreshToken dans un Cookie HttpOnly
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        var response = await _mediator.Send(query);

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(new AuthResponse(response.AccessToken));
    }

    // Renouvellement de l'AccessToken via le RefreshToken extrait du cookie
    [HttpPost("refresh_token")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"]
            ?? throw new UnauthorizedException("Refresh Token manquant dans les cookies.");

        var response = await _mediator.Send(new RefreshTokenCommand(refreshToken));

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(new AuthResponse(response.AccessToken));
    }

    // Déconnexion : révoque le token en BDD et supprime le cookie HTTP
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _mediator.Send(new RevokeTokenCommand(refreshToken));
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Déconnexion réussie." });
    }

    // Configuration sécurisée du cookie HttpOnly (XSS & CSRF protection)
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}