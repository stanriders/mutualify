using Mutualify.Contracts;
using Mutualify.Database.Models;

namespace Mutualify.Services.Interfaces;

public interface IRelationsService
{
    Task<List<RelationUser>> GetFriends(int userId, bool orderByRank);
    Task<UserFriendsContract> GetUsersFriends(int userId, bool orderByRank);
    Task<List<RelationUser>> GetFollowers(int userId, bool orderByRank);
    Task UpdateRelations(int userId);
    Task<long> GetRelationCount();
}
