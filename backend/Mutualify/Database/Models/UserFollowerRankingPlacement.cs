using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mutualify.Database.Models;

[Keyless]
public class UserFollowerRankingPlacement
{
    [Column("row_number")]
    public int RowNumber { get; set; }
}
