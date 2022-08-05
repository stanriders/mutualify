using Mutualify.OsuApi.Models;

namespace Mutualify.OsuApi.Interfaces;

public interface IOsuApiProvider
{
    Task<OsuUser?> GetUser(string token);
    Task<OsuUser[]?> GetFriends(string token);
    Task<TokenResponse?> RefreshToken(string refreshToken, string accessToken);
}
