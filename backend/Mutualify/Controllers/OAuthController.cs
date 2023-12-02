using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
    private readonly ILogger<OAuthController> _logger;
    private readonly IOsuApiProvider _osuApiDataService;
    private readonly IUserRepository _userRepository;
    private readonly IRelationsService _relationsService;
    private readonly IMapper _mapper;

    public OAuthController(ILogger<OAuthController> logger, 
    IOsuApiProvider osuApiDataService,
    IUserRepository userRepository,
    IMapper mapper,
    IRelationsService relationsService)
    {
        _logger = logger;
        _osuApiDataService = osuApiDataService;
        _userRepository = userRepository;
        _mapper = mapper;
        _relationsService = relationsService;
    }

    /// <summary>
    ///     osu! API authentication.
    /// </summary>
    [HttpGet("auth")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Authenticate()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("CompleteAuthentication", "OAuth")
        };

        return Challenge(authenticationProperties, "osu");
    }

    [HttpGet("complete")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> CompleteAuthentication()
    {
        var authResult = await HttpContext.AuthenticateAsync("ExternalCookies");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var accessToken = await HttpContext.GetTokenAsync("ExternalCookies", "access_token");
        var refreshToken = await HttpContext.GetTokenAsync("ExternalCookies", "refresh_token");

        OsuUser? osuUser;
        try
        {
            osuUser = await _osuApiDataService.GetUser(accessToken!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "osu!api provider error!");
            return Forbid();
        }

        if (osuUser is null || accessToken is null || refreshToken is null)
        {
            return Forbid();
        }

        if (osuUser.IsRestricted)
        {
            _logger.LogInformation("User {User} tried logging in, but they are restricted!", osuUser.Id);

            await HttpContext.SignOutAsync("ExternalCookies");

            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/restricted");
        }

        if (osuUser.IsRestricted || osuUser.IsBot || osuUser.IsDeleted)
        {
            _logger.LogInformation("User {User} tried logging in, but they are bot?!", osuUser.Id);

            return Forbid();
        }

        var user = await _userRepository.Get(osuUser.Id, true);
        if (user is null)
        {
            user = _mapper.Map<OsuUser, User>(osuUser);

            _logger.LogInformation("Adding user {Id} - {Username} to the database", osuUser.Id, osuUser.Username);

            await _userRepository.Add(user);
        }
        else
        {
            user.Username = osuUser.Username;
            user.CountryCode = osuUser.CountryCode;
            user.FollowerCount = osuUser.FollowerCount;
            user.Title = osuUser.Title;
            user.Rank = osuUser.Statistics?.GlobalRank;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);
        }

        await _userRepository.UpsertTokens(new Token
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        await _relationsService.UpdateRelations(user.Id);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, osuUser.Id.ToString()),
        };

        var id = new ClaimsIdentity(claims, "InternalCookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = authResult.Properties?.ExpiresUtc
        };

        await HttpContext.SignInAsync("InternalCookies", new ClaimsPrincipal(id), authProperties);
        await HttpContext.SignOutAsync("ExternalCookies");

        _logger.LogDebug("User {Username} logged in", osuUser.Username);
        
        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/");
    }

    /// <summary>
    ///     Sign out from current user.
    /// </summary>
    [Authorize]
    [HttpGet("signout")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("InternalCookies");

        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/");
    }
}
