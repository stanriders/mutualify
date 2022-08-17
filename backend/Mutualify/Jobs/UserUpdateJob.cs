using System.Net;
using Hangfire;
using Hangfire.Server;
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

    private const int _interval = 4; // seconds

    public UserUpdateJob(IUsersService usersService, IRelationsService relationsService, IUserRepository userRepository, ILogger<UserUpdateJob> logger)
    {
        _usersService = usersService;
        _relationsService = relationsService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Run(PerformContext context, IJobCancellationToken token)
    {
        var jobId = context.BackgroundJob.Id;

        _logger.LogInformation("[{JobId}] Starting user update job...", jobId);

        var userUpdateQueue = await _userRepository.GetAllIds();

        for (var i = 0; i < userUpdateQueue.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            var userId = userUpdateQueue[i];
            var startTime = DateTime.Now;

            try
            {
                var tokens = await _userRepository.GetTokens(userId);

                if (tokens is null)
                    continue;

#if !DEBUG
                if (i % 100 == 0)
#endif
                    _logger.LogInformation("[{JobId}] ({Current}/{Total}) Updating {Id}...", jobId, i+1,
                        userUpdateQueue.Count, userId);

                await _usersService.Update(userId);
                await _relationsService.UpdateRelations(userId);
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException httpRequestException &&
                    httpRequestException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogDebug("[{JobId}] User {User} updated their tokens, but still got 401 from API!", jobId,
                        userId);

                    Thread.Sleep(_interval * 1000);

                    continue;
                }

                throw;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogDebug("[{JobId}] User {User} updated their tokens, but still got 401 from API!", jobId,
                        userId);

                    Thread.Sleep(_interval * 1000);

                    continue;
                }

                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[{JobId}] User update job has been cancelled!", jobId);
                
                return;
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
