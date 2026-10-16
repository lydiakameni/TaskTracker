using FluentValidation;

namespace TaskTracker.Application.Features.Auth.Queries;

/// <summary>
/// Validation minimale de la connexion : les règles de complexité ne s'appliquent
/// qu'à l'inscription, pour ne pas bloquer les comptes créés avant leur mise en place.
/// </summary>
public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Le mot de passe est obligatoire.");
    }
}