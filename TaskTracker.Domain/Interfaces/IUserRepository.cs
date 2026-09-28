using TaskTracker.Domain.Entities;

namespace TaskTracker.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string token); // Récupérer le token
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken); // Sauvegarder la révocation
}