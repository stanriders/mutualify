using Newtonsoft.Json;

namespace Mutualify.OsuApi.Models
{
    public class GetUserlessTokenRequest
    {
        [JsonProperty("client_id")]
        public required int ClientId { get; set; }

        [JsonProperty("client_secret")]
        public required string ClientSecret { get; set; } = null!;

        [JsonProperty("grant_type")]
        public string GrantType { get; set; } = "client_credentials";

        [JsonProperty("scope")]
        public string Scope { get; set; } = "public";
    }
}
