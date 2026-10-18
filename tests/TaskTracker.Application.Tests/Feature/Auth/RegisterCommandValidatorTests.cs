using FluentValidation.TestHelper;
using TaskTracker.Application.Features.Auth.Commands.Register;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests des règles de validation de l'inscription.
/// </summary>
public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_AvecDesDonneesValides_NeRetourneAucuneErreur()
    {
        var result = _validator.TestValidate(new RegisterCommand("lydia_kameni", "MotDePasse1"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]             // vide
    [InlineData("ab")]           // trop court (moins de 3 caractères)
    [InlineData("lydia kameni")] // contient un espace
    [InlineData("lydia@mail")]   // caractère interdit
    public void Validate_AvecUnNomDUtilisateurInvalide_RetourneUneErreurSurUsername(string username)
    {
        var result = _validator.TestValidate(new RegisterCommand(username, "MotDePasse1"));

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]            // vide
    [InlineData("Court1")]      // moins de 8 caractères
    [InlineData("motdepasse1")] // aucune majuscule
    [InlineData("MOTDEPASSE1")] // aucune minuscule
    [InlineData("MotDePasse")]  // aucun chiffre
    public void Validate_AvecUnMotDePasseTropFaible_RetourneUneErreurSurPassword(string password)
    {
        var result = _validator.TestValidate(new RegisterCommand("lydia", password));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}