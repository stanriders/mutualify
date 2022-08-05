using System.Net;
using Mutualify.Jobs.Interfaces;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Jobs;

public class UserUpdateJob : IUserUpdateJob
{
    private List<int> _userUpdateQueue = null!;

    private readonly IUserRepository _userRepository;
    private readonly IUsersService _usersService;
    private readonly IRelationsService _relationsService;
    private readonly ILogger<UserUpdateJob> _logger;

    private const int _interval = 5;

    public UserUpdateJob(IUsersService usersService, IRelationsService relationsService, IUserRepository userRepository, ILogger<UserUpdateJob> logger)
    {
        _usersService = usersService;
        _relationsService = relationsService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public void Run()
    {
        _userUpdateQueue = _userRepository.GetAllIds().Result;

        foreach (var userId in _userUpdateQueue)
        {
            var startTime = DateTime.Now;

            try
            {
                _logger.LogInformation("Updating {Id}...", userId);
                _usersService.Update(userId).Wait();
                _relationsService.UpdateRelations(userId).Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException httpRequestException &&
                    httpRequestException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("User {User} updated their tokens, but still got 401 from API!", userId);

                    continue;
                }

                throw;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("User {User} updated their tokens, but still got 401 from API!", userId);

                    continue;
                }

                throw;
            }
            finally
            {
                var endTime = DateTime.Now;

                // yea its not really accurate but we don't need precision, just run it approximately every X seconds
                var elapsed = endTime - startTime;
                var timeout = elapsed.TotalSeconds < _interval ? _interval - (int) elapsed.TotalSeconds : 0;

                Thread.Sleep(timeout * 1000);
            }
        }
    }
}
