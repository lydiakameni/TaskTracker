using MediatR;
using TaskTracker.Application.Features.Auth.DTOs;

namespace TaskTracker.Application.Features.Auth.Commands;



// La Commande contient les données envoyées par l'utilisateur
public record RegisterCommand(string Username, string Password, string Role= "User") : IRequest<AuthResponse>;

