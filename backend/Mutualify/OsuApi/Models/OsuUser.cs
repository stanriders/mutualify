using System.Text.Json.Serialization;

namespace Mutualify.OsuApi.Models;

public class OsuUser
{
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = null!;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("is_restricted")]
    public bool IsRestricted { get; set; }
    
    [JsonPropertyName("follower_count")]
    public int FollowerCount { get; set; }
}
