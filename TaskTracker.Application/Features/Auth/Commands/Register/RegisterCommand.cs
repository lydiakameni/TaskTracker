using MediatR;
using TaskTracker.Application.Features.Auth.DTOs;

namespace TaskTracker.Application.Features.Auth.Commands.Register;

/// <summary>
/// Commande d'inscription d'un nouvel utilisateur.
/// </summary>
/// <remarks>
/// Le rôle n'est volontairement pas exposé : tout nouvel inscrit reçoit le rôle "User".
/// Accepter un rôle venant du client permettrait à n'importe qui de devenir administrateur.
/// </remarks>
/// <param name="Username">Nom d'utilisateur unique.</param>
/// <param name="Password">Mot de passe en clair, haché avec BCrypt avant stockage.</param>
public record RegisterCommand(string Username, string Password) : IRequest<AuthenticationResult>;