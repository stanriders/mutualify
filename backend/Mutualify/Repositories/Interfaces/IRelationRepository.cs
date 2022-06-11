using Mutualify.Database.Models;

namespace Mutualify.Repositories.Interfaces;

public interface IRelationRepository
{
    Task<List<Relation>> Get(int userId);
    Task<List<Relation>> GetFollowers(int userId);

    Task Add(List<Relation> relations);
    Task Remove(int userId);

    Task ReplaceRelations(int userId, List<Relation> relations);
}
