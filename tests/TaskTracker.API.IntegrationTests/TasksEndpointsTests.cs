using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskTracker.API.IntegrationTests;

/// <summary>
/// Tests de bout en bout des routes de gestion des tâches (/api/tasks) :
/// rôles, propriétaire de la tâche et codes HTTP.
/// </summary>
public class TasksEndpointsTests : IClassFixture<TaskTrackerApiFactory>
{
    private const string Password = "MotDePasse1";
    private readonly TaskTrackerApiFactory _factory;

    public TasksEndpointsTests(TaskTrackerApiFactory factory)
    {
        _factory = factory;
    }

    private record AuthResponseBody(string AccessToken);
    private record TaskBody(int Id, string Title, bool IsCompleted, int UserId);

    private static string NewUsername() => $"user_{Guid.NewGuid():N}"[..20];

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    /// <summary>Connecte un utilisateur et renvoie un client qui envoie son Access Token.</summary>
    private async Task<HttpClient> LoginAsync(string username)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password = Password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    /// <summary>Inscrit un nouvel utilisateur (rôle User) et renvoie son client connecté.</summary>
    private async Task<(HttpClient Client, string Username)> CreateUserAsync()
    {
        var username = NewUsername();
        var response = await CreateClient().PostAsJsonAsync("/api/auth/register", new { username, password = Password });
        response.EnsureSuccessStatusCode();
        return (await LoginAsync(username), username);
    }

    /// <summary>Crée un administrateur en base et renvoie son client connecté.</summary>
    private async Task<HttpClient> CreateAdminAsync()
    {
        var username = NewUsername();
        await _factory.CreateAdminAsync(username, Password);
        return await LoginAsync(username);
    }

    [Fact]
    public async Task CreateTask_ParUnUtilisateurSimple_Retourne403()
    {
        var (user, _) = await CreateUserAsync();

        var response = await user.PostAsJsonAsync("/api/tasks", new { title = "Tentative" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ParUnAdmin_Retourne201EtLaTacheEstConsultable()
    {
        var admin = await CreateAdminAsync();

        // Act : création
        var response = await admin.PostAsJsonAsync("/api/tasks", new { title = "Préparer la démo" });

        // Assert : 201 avec l'adresse de la nouvelle tâche dans l'en-tête Location
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        // La tâche est bien consultable à cette adresse
        var created = await admin.GetFromJsonAsync<TaskBody>(response.Headers.Location);
        Assert.Equal("Préparer la démo", created!.Title);
    }

    [Fact]
    public async Task ScenarioComplet_AssignationConsultationEtCompletionParLeProprietaire()
    {
        // Arrange : un admin assigne une tâche à un utilisateur
        var admin = await CreateAdminAsync();
        var (user, username) = await CreateUserAsync();
        var userId = await _factory.GetUserIdAsync(username);

        var createResponse = await admin.PostAsJsonAsync("/api/tasks", new { title = "Rédiger le rapport", targetUserId = userId });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskBody>();

        // L'utilisateur voit la tâche dans sa liste
        var myTasks = await user.GetFromJsonAsync<List<TaskBody>>("/api/tasks");
        Assert.Contains(myTasks!, t => t.Id == task!.Id);

        // Act : il la marque comme terminée
        var completeResponse = await user.PatchAsync($"/api/tasks/{task!.Id}/complete", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<TaskBody>();
        Assert.True(completed!.IsCompleted);
    }

    [Fact]
    public async Task GetTaskById_ParUnAutreUtilisateur_Retourne404()
    {
        // Arrange : une tâche appartenant à l'admin
        var admin = await CreateAdminAsync();
        var createResponse = await admin.PostAsJsonAsync("/api/tasks", new { title = "Tâche privée" });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskBody>();

        var (otherUser, _) = await CreateUserAsync();

        // Act
        var response = await otherUser.GetAsync($"/api/tasks/{task!.Id}");

        // Assert : 404 et non 403, pour ne pas révéler que la tâche existe
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ParUnAdmin_Retourne204EtLaTacheDevientIntrouvable()
    {
        // Arrange
        var admin = await CreateAdminAsync();
        var createResponse = await admin.PostAsJsonAsync("/api/tasks", new { title = "À supprimer" });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskBody>();

        // Act
        var deleteResponse = await admin.DeleteAsync($"/api/tasks/{task!.Id}");

        // Assert : 204, puis la tâche (supprimée logiquement) n'est plus visible
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getResponse = await admin.GetAsync($"/api/tasks/{task.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}