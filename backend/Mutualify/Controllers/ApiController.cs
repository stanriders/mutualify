using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mutualify.Contracts;
using Mutualify.Database.Models;
using Mutualify.Services.Interfaces;

namespace Mutualify.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly IRelationsService _relationsService;
        private readonly IUsersService _usersService;
        private readonly ILogger<ApiController> _logger;

        private int _claim => int.Parse(HttpContext.User.Identity!.Name!);

        public ApiController(IRelationsService relationsService,
            IUsersService usersService,
            ILogger<ApiController> logger)
        {
            _relationsService = relationsService;
            _usersService = usersService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("/me")]
        public Task<User> GetSelf()
        {
            return _usersService.Get(_claim)!;
        }

        [Authorize]
        [HttpGet("/friends")]
        public Task<List<RelationUser>> GetFriends()
        {
            return _relationsService.GetFriends(_claim);
        }

        [HttpGet("/friends/{id}")]
        public Task<UserFriendsContract> GetFriendsById(int id)
        {
            return _relationsService.GetUsersFriends(id);
        }

        [Authorize]
        [HttpGet("/followers")]
        public Task<List<RelationUser>> GetFriendedBy()
        {
            return _relationsService.GetFollowers(_claim);
        }

        [HttpGet("/rankings")]
        public Task<RankingsContract> GetFollowerRanking(int offset = 0)
        {
            return _usersService.GetFollowerLeaderboard(offset);
        }

        [Authorize]
        [HttpGet("/rankings/me")]
        public Task<int> GetFollowerRankingForUser()
        {
            return _usersService.GetFollowerLeaderboardRanking(_claim);
        }

        [HttpGet("/stats")]
        public Task<StatsContract> GetStats()
        {
            return _usersService.GetStats();
        }

        [Authorize]
        [HttpPost("/friends/access/toggle")]
        public Task ToggleFriendlistAccess([FromBody] bool allow)
        {
            return _usersService.ToggleFriendlistAccess(_claim, allow);
        }

        [Authorize]
        [HttpPost("/friends/refresh")]
        public async Task RefreshFriends()
        {
            try
            {
                await _usersService.Update(_claim, true);
                await _relationsService.UpdateRelations(_claim);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("User {User} updated their tokens, but still got 401 from API!", _claim);
                    await HttpContext.SignOutAsync("InternalCookies");

                    return;
                }

                throw;
            }
        }
    }
}
