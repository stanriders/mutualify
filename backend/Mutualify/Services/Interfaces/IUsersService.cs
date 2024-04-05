
using Mutualify.Contracts;
using Mutualify.Database.Models;

namespace Mutualify.Services.Interfaces
{
    public interface IUsersService
    {
        Task<User?> Get(int userId);
        Task<StatsContract> GetStats();
        Task<RankingsContract> GetFollowerLeaderboard(int offset);
        Task<int> GetFollowerLeaderboardRanking(int userId);
        Task Update(int userId);
        Task ToggleFriendlistAccess(int userId, bool allow);
    }
}
