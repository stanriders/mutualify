using System.Net;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOsuApiProvider _osuApiDataService;

        public UsersService(IUserRepository userRepository, IOsuApiProvider osuApiDataService)
        {
            _userRepository = userRepository;
            _osuApiDataService = osuApiDataService;
        }

        public async Task ToggleFriendlistAccess(int userId, bool allow)
        {
            var user = await _userRepository.Get(userId, true);
            if (user is not null)
            {
                user.AllowsFriendlistAccess = allow;
                await _userRepository.Update(user);
            }
        }

        public async Task Update(int userId)
        {
            var token = await _userRepository.GetTokens(userId);
            if (token is null)
                return;

            OsuUser? osuUser = null;

            try
            {
                osuUser = await _osuApiDataService.GetUser(token.AccessToken);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var newToken = await _osuApiDataService.RefreshToken(token.RefreshToken, token.AccessToken);
                    if (newToken is not null)
                    {
                        await _userRepository.UpsertTokens(new Token
                        {
                            UserId = userId,
                            AccessToken = newToken.AccessToken,
                            RefreshToken = newToken.RefreshToken
                        });

                        osuUser = await _osuApiDataService.GetUser(newToken.AccessToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (osuUser is null)
                return;

            var user = await _userRepository.Get(userId, true);
            if (user is not null)
            {
                user.Username = osuUser.Username;
                user.CountryCode = osuUser.CountryCode;
                user.FollowerCount = osuUser.FollowerCount;
                user.Title = osuUser.Title;

                await _userRepository.Update(user);
            }
        }
    }
}
