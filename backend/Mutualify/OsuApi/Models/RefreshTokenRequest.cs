using Newtonsoft.Json;

namespace Mutualify.OsuApi.Models;

public class RefreshTokenRequest
{
    [JsonProperty("client_id")]
    public int ClientId { get; set; }

    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; } = null!;

    [JsonProperty("grant_type")]
    public string GrantType { get; set; } = "refresh_token";

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = null!;
}
