using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mutualify.Contracts;
using Mutualify.Database.Models;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly IRelationsService _relationsService;
        private readonly IUserRepository _userRepository;

        private int _claim => int.Parse(HttpContext.User.Identity!.Name!);

        public ApiController(IRelationsService relationsService,
            IUserRepository userRepository)
        {
            _relationsService = relationsService;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet("/me")]
        public Task<User> GetSelf()
        {
            return _userRepository.Get(_claim)!;
        }

        [Authorize]
        [HttpGet("/friends")]
        public Task<List<User>> GetFriends()
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
        public Task<List<User>> GetFriendedBy(bool filterMutuals)
        {
            return _relationsService.GetFollowers(_claim, filterMutuals);
        }

        [HttpGet("/rankings")]
        public Task<List<User>> GetFollowerRanking()
        {
            return _userRepository.GetFollowerRanking();
        }

        [HttpGet("/stats")]
        public async Task<StatsContract> GetStats()
        {
            return new StatsContract
            {
                RegisteredCount = await _userRepository.GetRegisteredUserCount(),
                RelationCount = await _relationsService.GetRelationCount()
            };
        }

        [Authorize]
        [HttpPost("/friends/access/toggle")]
        public Task ToggleFriendlistAccess([FromBody] bool allow)
        {
            return _relationsService.ToggleFriendlistAccess(_claim, allow);
        }

        [Authorize]
        [HttpPost("/friends/refresh")]
        public Task RefreshFriends()
        {
            return _relationsService.UpdateRelations(_claim);
        }
    }
}
