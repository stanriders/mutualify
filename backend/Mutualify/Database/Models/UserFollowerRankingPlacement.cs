using Microsoft.EntityFrameworkCore;

namespace Mutualify.Database.Models;

[Keyless]
public class UserFollowerRankingPlacement
{
    public int RowNumber { get; set; }
}
