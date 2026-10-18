using FluentValidation.TestHelper;
using TaskTracker.Application.Features.Tasks.Commands.CreateTask;

namespace TaskTracker.Application.Tests.Features.Tasks;

/// <summary>
/// Tests des règles de validation de la création de tâche.
/// </summary>
public class CreateTaskCommandValidatorTests
{
    private const int AdminId = 1;
    private readonly CreateTaskCommandValidator _validator = new();

    [Theory]
    [InlineData(null)] // sans utilisateur cible : la tâche est pour l'admin
    [InlineData(3)]    // avec un utilisateur cible valide
    public void Validate_AvecDesDonneesValides_NeRetourneAucuneErreur(int? targetUserId)
    {
        var result = _validator.TestValidate(new CreateTaskCommand("Préparer la démo", AdminId, targetUserId));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]    // vide
    [InlineData("   ")] // uniquement des espaces
    public void Validate_AvecUnTitreVide_RetourneUneErreurSurTitle(string title)
    {
        var result = _validator.TestValidate(new CreateTaskCommand(title, AdminId));

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_AvecUnTitreDePlusDe200Caracteres_RetourneUneErreurSurTitle()
    {
        var titreTropLong = new string('a', 201);

        var result = _validator.TestValidate(new CreateTaskCommand(titreTropLong, AdminId));

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AvecUnIdentifiantCibleInvalide_RetourneUneErreurSurTargetUserId(int targetUserId)
    {
        var result = _validator.TestValidate(new CreateTaskCommand("Préparer la démo", AdminId, targetUserId));

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId);
    }
}