using Mutualify.Database.Models;

namespace Mutualify.Contracts
{
    public class UserFriendsContract
    {
        public User? User { get; set; }
        public List<RelationUser> Friends { get; set; } = new();
    }
}
