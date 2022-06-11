
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Mutualify.Database.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    public string CountryCode { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? Title { get; set; }

    public int FollowerCount { get; set; }

    [InverseProperty(nameof(Relation.To))]
    [JsonIgnore]
    public List<Relation> ToRelations { get; set; } = null!;

    [InverseProperty(nameof(Relation.From))]
    [JsonIgnore]
    public List<Relation> FromRelations { get; set; } = null!;

    [JsonIgnore]
    public Token Token { get; set; } = null!;
}
