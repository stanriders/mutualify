namespace Mutualify.Configuration;

public class OsuApiConfig
{
    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = null!;
    public string CallbackUrl { get; set; } = null!;
}
