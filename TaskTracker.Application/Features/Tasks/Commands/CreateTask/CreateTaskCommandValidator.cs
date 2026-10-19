using FluentValidation;

namespace TaskTracker.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Règles de validation de la création d'une tâche.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre de la tâche est obligatoire.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.TargetUserId)
            .GreaterThan(0).WithMessage("L'identifiant de l'utilisateur cible est invalide.")
            .When(x => x.TargetUserId.HasValue);
    }
}