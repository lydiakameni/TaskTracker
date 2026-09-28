using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Security;

public class JwtProvider : IJwtProvider
{
    // IConfiguration permet de lire le fichier appsettings.json de l'API 
    // pour récupérer notre clé secrète sans l'écrire en dur dans le code.
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        // 1. Les Claims : Les données qu'on embarque dans le token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Sub = Subject (l'ID de l'utilisateur)
            new Claim(ClaimTypes.Name, user.Username),                  // Son nom
            new Claim(ClaimTypes.Role, user.Role)                       // Son rôle (pour restreindre certains accès plus tard)
        };

        // 2. La clé secrète : On la récupère depuis la configuration et on la transforme en bytes
        var secretKey = _configuration["Jwt:SecretKey"] ?? throw new Exception("Clé JWT manquante");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // 3. L'algorithme de signature : On utilise HmacSha256 (le standard robuste actuel)
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Configuration finale du token (durée de vie très courte pour la sécurité : 15 minutes)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            //Expires= DateTime.UtcNow.AddSeconds(10),  /*Pour le test*/
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],     // Qui a émis le token ? (ex: "TaskTrackerAPI")
            Audience = _configuration["Jwt:Audience"]  // Pour qui est-il destiné ? (ex: "TaskTrackerFrontEnd")
        };

        // 5. Génération de la chaîne de caractères finale
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        
        return handler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(int userId)
    {
        // Le refresh token n'est pas un JWT. C'est juste une chaîne de caractères 
        // totalement aléatoire et unique, stockée en base de données.
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Durée de vie longue (7 jours)
            UserId = userId
        };
    }
}