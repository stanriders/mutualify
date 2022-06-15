using Mutualify.Database.Models;

namespace Mutualify.Contracts
{
    public class RankingsContract
    {
        public int Total { get; set; }
        public List<User> Users { get; set; } = new();
    }
}
