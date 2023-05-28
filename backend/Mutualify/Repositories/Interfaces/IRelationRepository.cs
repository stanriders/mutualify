using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IRelationRepository
{
    Task<List<RelationUser>> GetFriends(int userId, bool highlightMutuals, bool orderByRank);
    Task<List<RelationUser>> GetFollowers(int userId, bool orderByRank);
    
    Task ReplaceRelations(int userId, List<Relation> relations);

    Task<long> GetRelationCount();
}
