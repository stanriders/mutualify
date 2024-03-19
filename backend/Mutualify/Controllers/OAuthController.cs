using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Mutualify.Services.Interfaces;

namespace Mutualify.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
    private readonly ILogger<OAuthController> _logger;
    private readonly IOsuApiProvider _osuApiDataService;
    private readonly DatabaseContext _databaseContext;
    private readonly IRelationsService _relationsService;
    private readonly IMapper _mapper;

    public OAuthController(ILogger<OAuthController> logger, 
    IOsuApiProvider osuApiDataService,
    IMapper mapper,
    IRelationsService relationsService,
    DatabaseContext databaseContext)
    {
        _logger = logger;
        _osuApiDataService = osuApiDataService;
        _mapper = mapper;
        _relationsService = relationsService;
        _databaseContext = databaseContext;
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

            var dbUser = await _databaseContext.Users.FindAsync(osuUser.Id);
            if (dbUser is not null)
            {
                // remove the account if they're restricted
                _databaseContext.Users.Remove(dbUser);
                await _databaseContext.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync("ExternalCookies");

            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/restricted");
        }

        if (osuUser.IsBot || osuUser.IsDeleted)
        {
            _logger.LogInformation("User {User} tried logging in, but they are a bot?!", osuUser.Id);

            return Forbid();
        }

        var existingUser = await _databaseContext.Users.FindAsync(osuUser.Id);
        if (existingUser is null)
        {
            var newUser = _mapper.Map<OsuUser, User>(osuUser);
            newUser.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Adding user {Id} - {Username} to the database", osuUser.Id, osuUser.Username);

            await _databaseContext.Users.AddAsync(newUser);
        }
        else
        {
            existingUser.Username = osuUser.Username;
            existingUser.CountryCode = osuUser.CountryCode;
            existingUser.FollowerCount = osuUser.FollowerCount;
            existingUser.Title = osuUser.Title;
            existingUser.Rank = osuUser.Statistics?.GlobalRank;
            existingUser.UpdatedAt = DateTime.UtcNow;

            _databaseContext.Users.Update(existingUser);
        }

        var existingTokens = await _databaseContext.Tokens.FindAsync(osuUser.Id);
        if (existingTokens is not null)
        {
            existingTokens.AccessToken = accessToken;
            existingTokens.RefreshToken = refreshToken;
            
            _databaseContext.Tokens.Update(existingTokens);
        }
        else
        {
            await _databaseContext.Tokens.AddAsync(new Token
            {
                UserId = osuUser.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        await _databaseContext.SaveChangesAsync();

        await _relationsService.UpdateRelations(osuUser.Id);

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
