using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> Get(int id, bool track = false);
    Task<List<User>> Get(List<int> ids);
    Task<User> Add(User user);
    Task UpsertRange(List<User> users);
    Task Update(User user);
    Task Remove(User user);

    Task<Token?> GetTokens(int userId);
    Task UpsertTokens(Token token);
}
