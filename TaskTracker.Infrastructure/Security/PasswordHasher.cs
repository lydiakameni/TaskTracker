using BCrypt.Net;
using TaskTracker.Application.Interfaces; // On importe le contrat défini dans l'Application

namespace TaskTracker.Infrastructure.Security;

// Cette classe "signe" le contrat IPasswordHasher
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        // BCrypt génère un sel automatiquement et hache le mot de passe.
        // Le résultat est une chaîne indéchiffrable (ex: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy)
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        // BCrypt extrait le sel du hash stocké en BDD et vérifie si 
        // le mot de passe fourni par l'utilisateur donne le même résultat.
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}