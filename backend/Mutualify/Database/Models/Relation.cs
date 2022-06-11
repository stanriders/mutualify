
using System.ComponentModel.DataAnnotations.Schema;

namespace Mutualify.Database.Models
{
    public class Relation
    {
        [ForeignKey("From")]
        public int FromId { get; set; }

        [ForeignKey("To")]
        public int ToId { get; set; }

        public User From { get; set; } = null!;

        public User To { get; set; } = null!;
    }
}
