
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Mutualify.Configuration;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Newtonsoft.Json;

namespace Mutualify.OsuApi;

public class OsuApiProvider : IOsuApiProvider
{
    private const string _osuBase = "https://osu.ppy.sh/";
    private const string _apiMeLink = "api/v2/me/";
    private const string _apiUserLink = "api/v2/users/{0}?key=id";
    private const string _apiFriendsLink = "api/v2/friends/";
    private const string _apiTokenLink = "oauth/token";

    private readonly HttpClient _httpClient;
    private readonly OsuApiConfig _config;
    private readonly ILogger<OsuApiProvider> _logger;

    private TokenResponse? _userlessToken;
    private DateTime? _userlessTokenExpiration;

    public OsuApiProvider(IOptions<OsuApiConfig> config, HttpClient httpClient, ILogger<OsuApiProvider> logger)
    {
        _config = config.Value;
        _httpClient = httpClient;
        _logger = logger;

        RefreshUserlessToken().Wait();
    }

    public async Task<OsuUser?> GetUser(string token)
    {
        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_osuBase + _apiMeLink),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };
        
        var response = await _httpClient.SendAsync(requestMessage);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OsuUser>();
    }

    public async Task<OsuUser?> GetUser(int id)
    {
        await RefreshUserlessToken();

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_osuBase + string.Format(_apiUserLink, id)),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _userlessToken!.AccessToken) }
        };

        var response = await _httpClient.SendAsync(requestMessage);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OsuUser>();
    }

    public async Task<OsuUser[]?> GetFriends(string token)
    {
        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_osuBase + _apiFriendsLink),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        var response = await _httpClient.SendAsync(requestMessage);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OsuUser[]>();
    }

    public async Task<TokenResponse?> RefreshToken(string refreshToken, string accessToken)
    {
        var config = new RefreshTokenRequest
        {
            ClientId = _config.ClientId,
            ClientSecret = _config.ClientSecret,
            RefreshToken = refreshToken,
            AccessToken = accessToken
        };

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_osuBase + _apiTokenLink),
            Content = new StringContent(JsonConvert.SerializeObject(config), null, "application/json"),
            Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") }}
        };

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    private async Task RefreshUserlessToken()
    {
        if (_userlessTokenExpiration > DateTime.UtcNow)
        {
            return;
        }

        var requestModel = new GetUserlessTokenRequest
        {
            ClientId = _config.ClientId,
            ClientSecret = _config.ClientSecret,
        };

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_osuBase + _apiTokenLink),
            Content = new StringContent(JsonConvert.SerializeObject(requestModel), null, "application/json"),
            Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
        };

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.Log(LogLevel.Error, "Couldn't update userless token! Status code: {Code}", response.StatusCode);
            return;
        }

        _userlessToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (_userlessToken == null)
        {
            _logger.Log(LogLevel.Error, "Couldn't parse userless token! {Json}", await response.Content.ReadAsStringAsync());
            return;
        }

        _userlessTokenExpiration = DateTime.UtcNow.AddSeconds(_userlessToken.ExpiresIn);
    }
}
