using Mutualify.Contracts;
using Mutualify.Database.Models;

namespace Mutualify.Services.Interfaces;

public interface IRelationsService
{
    Task<List<User>> GetFriends(int userId);
    Task<UserFriendsContract> GetUsersFriends(int userId);
    Task<List<User>> GetFollowers(int userId, bool filterMutuals);
    Task UpdateRelations(int userId);
    Task<long> GetRelationCount();
}
