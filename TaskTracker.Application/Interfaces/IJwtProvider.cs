using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(int userId);
}