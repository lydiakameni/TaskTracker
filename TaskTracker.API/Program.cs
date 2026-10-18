using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TaskTracker.Application;
using TaskTracker.Infrastructure;
using TaskTracker.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. ENREGISTREMENT DES DÉPENDANCES (SERVICES)
// ==========================================================
// Enregistrement du service ExceptionHandler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // Permet le formatage standardisé


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskTracker API",
        Version = "v1",
        Description = "API REST de gestion de tâches : authentification JWT avec rotation des Refresh Tokens, " +
                    "rôles Admin/User, Clean Architecture et CQRS (MediatR).",
        Contact = new OpenApiContact
        {
            Name = "Lydia Garcia MBOUWE KAMENI",
            Url = new Uri("https://github.com/LydiaKameni")
        }
    });

    // Intègre les commentaires XML (///) de l'API et de la couche Application dans Swagger
    foreach (var xmlFile in new[] { "TaskTracker.API.xml", "TaskTracker.Application.xml" })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Collez directement votre Access Token JWT ci-dessous."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // Le document est passé à la référence pour qu'elle pointe bien vers le schéma "Bearer"
    // déclaré ci-dessus. Sans lui, Swagger UI n'envoie pas l'en-tête Authorization.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Configuration de l'Authentification JWT
// La clé de signature JWT ne doit jamais être stockée dans le code source.
// Développement : dotnet user-secrets set "Jwt:SecretKey" "<clé>" --project TaskTracker.API
// Production    : variable d'environnement Jwt__SecretKey
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || Encoding.UTF8.GetByteCount(jwtSecretKey) < 32)
{
    throw new InvalidOperationException(
        "La clé 'Jwt:SecretKey' est absente ou trop courte (32 octets minimum pour HMAC-SHA256). " +
        "Configurez-la avec 'dotnet user-secrets' ou la variable d'environnement 'Jwt__SecretKey'.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero 
        };
    });

// ==========================================================
// 2. CONSTRUCTION DE L'APPLICATION
// ==========================================================
var app = builder.Build();

// ==========================================================
// 3. PIPELINE HTTP (MIDDLEWARES)
// ==========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Activation du middleware en HAUT du pipeline HTTP
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

app.Run();

/// <summary>
/// Point d'entrée de l'API. Déclaré <c>partial</c> et <c>public</c> pour permettre
/// aux tests d'intégration de démarrer l'application avec <c>WebApplicationFactory</c>.
/// </summary>
public partial class Program { }