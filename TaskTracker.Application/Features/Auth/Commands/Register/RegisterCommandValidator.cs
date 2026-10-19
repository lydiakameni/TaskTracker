using FluentValidation;

namespace TaskTracker.Application.Features.Auth.Commands.Register;

/// <summary>
/// Règles de validation de l'inscription : nom d'utilisateur lisible
/// et mot de passe suffisamment robuste.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // Règles de validation pour le nom d'utilisateur et le mot de passe, 
        // avec des messages d'erreur explicites pour l'utilisateur final.
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire.")
            .Length(3, 50).WithMessage("Le nom d'utilisateur doit contenir entre 3 et 50 caractères.")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Le nom d'utilisateur ne peut contenir que des lettres, chiffres, '.', '_' et '-'.");

        // Règles de validation pour le mot de passe : longueur minimale et complexité
        // (au moins une majuscule, une minuscule et un chiffre)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule.")
            .Matches("[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule.")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.");
    }
}