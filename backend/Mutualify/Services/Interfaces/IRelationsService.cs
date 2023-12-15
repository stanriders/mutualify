using Mutualify.Contracts;
using Mutualify.Database.Models;

namespace Mutualify.Services.Interfaces;

public interface IRelationsService
{
    Task<List<RelationUser>> GetFriends(int userId);
    Task<UserFriendsContract> GetUsersFriends(int userId);
    Task<List<RelationUser>> GetFollowers(int userId);
    Task UpdateRelations(int userId);
    Task<long> GetRelationCount();
}
