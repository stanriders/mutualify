using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IRelationRepository
{
    Task<List<User>> GetFriends(int userId);
    Task<List<User>> GetFollowers(int userId, bool filterMutuals);

    Task Add(List<Relation> relations);
    Task Remove(int userId);

    Task ReplaceRelations(int userId, List<Relation> relations);
}
