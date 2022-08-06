
namespace Mutualify.Services.Interfaces
{
    public interface IUsersService
    {
        Task Update(int userId);
        Task ToggleFriendlistAccess(int userId, bool allow);
    }
}
