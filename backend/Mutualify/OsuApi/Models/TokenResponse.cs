using Newtonsoft.Json;

namespace Mutualify.OsuApi.Models;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = null!;
}
