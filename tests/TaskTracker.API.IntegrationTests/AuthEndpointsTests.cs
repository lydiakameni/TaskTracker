using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskTracker.API.IntegrationTests;

/// <summary>
/// Tests de bout en bout des routes d'authentification (/api/auth).
/// </summary>
public class AuthEndpointsTests : IClassFixture<TaskTrackerApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TaskTrackerApiFactory factory)
    {
        // HTTPS : le cookie du Refresh Token est marqué Secure
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>Génère un nom d'utilisateur unique, pour que les tests restent indépendants.</summary>
    private static string NewUsername() => $"user_{Guid.NewGuid():N}"[..20];

    private record AuthResponseBody(string AccessToken);

    [Fact]
    public async Task Register_AvecDesDonneesValides_Retourne200AvecAccessTokenEtCookieHttpOnly()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { username = NewUsername(), password = "MotDePasse1" });

        // Assert : 200 et Access Token dans le corps
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
        Assert.False(string.IsNullOrWhiteSpace(body?.AccessToken));

        // Le Refresh Token est dans un cookie HttpOnly, donc inaccessible au JavaScript
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith("refreshToken="));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_AvecUnNomDejaPris_Retourne409()
    {
        // Arrange : un premier compte avec ce nom
        var username = NewUsername();
        await _client.PostAsJsonAsync("/api/auth/register", new { username, password = "MotDePasse1" });

        // Act : deuxième inscription avec le même nom
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { username, password = "MotDePasse1" });

        // Assert : 409 au format ProblemDetails
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Register_AvecUnMotDePasseFaible_Retourne400AvecLErreurSurLeChampPassword()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { username = NewUsername(), password = "faible" });

        // Assert : 400 avec la liste des erreurs par champ
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = json.RootElement.GetProperty("errors");
        Assert.Contains(errors.EnumerateObject(),
            e => e.Name.Equals("Password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_AvecUnMauvaisMotDePasse_Retourne401()
    {
        // Arrange
        var username = NewUsername();
        await _client.PostAsJsonAsync("/api/auth/register", new { username, password = "MotDePasse1" });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username, password = "Mauvais1" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_SansToken_Retourne401()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}