using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mutualify.Database.Models;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IRelationsService _relationsService;
        private readonly IUserRepository _userRepository;

        private int _claim => int.Parse(HttpContext.User.Identity!.Name!);

        public ApiController(ILogger<ApiController> logger,
            IRelationsService relationsService,
            IUserRepository userRepository)
        {
            _logger = logger;
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

        [Authorize]
        [HttpGet("/followers")]
        public Task<List<User>> GetFriendedBy()
        {
            return _relationsService.GetFollowers(_claim);
        }
    }
}
