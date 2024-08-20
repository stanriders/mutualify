
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Mutualify.Database.Models;

public class User
{
    [Key]
    public required int Id { get; set; }

    public required string CountryCode { get; set; } = null!;

    public required string Username { get; set; } = null!;

    public string? Title { get; set; }

    public required int FollowerCount { get; set; }

    public int? Rank { get; set; }

    public bool AllowsFriendlistAccess { get; set; } = false;

    public required DateTime? CreatedAt { get; set; }

    public required DateTime? UpdatedAt { get; set; }

    [InverseProperty(nameof(Relation.To))]
    [JsonIgnore]
    public List<Relation> ToRelations { get; set; } = null!;

    [InverseProperty(nameof(Relation.From))]
    [JsonIgnore]
    public List<Relation> FromRelations { get; set; } = null!;

    [JsonIgnore]
    public Token? Token { get; set; }
}
