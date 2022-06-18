namespace Mutualify.Database.Models
{
    public class RelationUser
    {
        public int Id { get; set; }

        public string CountryCode { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string? Title { get; set; }

        public bool Mutual { get; set; }
    }
}
