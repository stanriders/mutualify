using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> Get(int id, bool track = false);
    Task<List<User>> Get(List<int> ids);
    Task<List<int>> GetAllIds();
    Task<List<User>> GetFollowerRanking(int limit = 50, int offset = 0);
    Task<int> GetUserFollowerRankingPlacement(int userId);
    Task<int> GetRegisteredUserCount();
    Task<User> Add(User user);
    Task UpsertRange(List<User> users);
    Task Update(User user);
    Task Remove(User user);

    Task<Token?> GetTokens(int userId);
    Task UpsertTokens(Token token);
    Task RemoveTokens(int userId);
}
