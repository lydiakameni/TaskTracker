using FluentValidation.TestHelper;
using TaskTracker.Application.Features.Auth.Queries;

namespace TaskTracker.Application.Tests.Features.Auth;

/// <summary>
/// Tests des règles de validation de la connexion.
/// </summary>
public class LoginQueryValidatorTests
{
    private readonly LoginQueryValidator _validator = new();

    [Fact]
    public void Validate_AvecDesChampsRemplis_NeRetourneAucuneErreur()
    {
        // Pas de règle de complexité à la connexion : un ancien mot de passe simple reste accepté
        var result = _validator.TestValidate(new LoginQuery("lydia", "123456"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SansNomDUtilisateur_RetourneUneErreurSurUsername()
    {
        var result = _validator.TestValidate(new LoginQuery("", "MotDePasse1"));

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_SansMotDePasse_RetourneUneErreurSurPassword()
    {
        var result = _validator.TestValidate(new LoginQuery("lydia", ""));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}