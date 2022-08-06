using System.Net;
using Mutualify.Jobs.Interfaces;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Jobs;

public class UserUpdateJob : IUserUpdateJob
{
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
        var jobId = Guid.NewGuid();

        _logger.LogInformation("[{JobId}] Starting user update job...", jobId);

        var userUpdateQueue = _userRepository.GetAllIds().Result;

        for (var i = 0; i < userUpdateQueue.Count; i++)
        {
            var userId = userUpdateQueue[i];
            var startTime = DateTime.Now;

            try
            {
                var tokens = _userRepository.GetTokens(userId).Result;
                if (tokens is null)
                    continue;

                _logger.LogInformation("[{JobId}] ({Current}/{Total}) Updating {Id}...", jobId, i, userUpdateQueue.Count, userId);
                _usersService.Update(userId).Wait();
                _relationsService.UpdateRelations(userId).Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException httpRequestException &&
                    httpRequestException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[{JobId}] User {User} updated their tokens, but still got 401 from API!", jobId, userId);

                    Thread.Sleep(_interval * 1000);

                    continue;
                }

                throw;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[{JobId}] User {User} updated their tokens, but still got 401 from API!", jobId, userId);

                    Thread.Sleep(_interval * 1000);

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

        _logger.LogInformation("[{JobId}] Finished user update job", jobId);
    }
}
