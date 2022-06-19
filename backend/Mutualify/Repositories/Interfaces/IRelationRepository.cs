using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IRelationRepository
{
    Task<List<RelationUser>> GetFriends(int userId, bool highlightMutuals);
    Task<List<RelationUser>> GetFollowers(int userId);

    Task Add(List<Relation> relations);
    Task Remove(int userId);

    Task ReplaceRelations(int userId, List<Relation> relations);

    Task<long> GetRelationCount();
}
