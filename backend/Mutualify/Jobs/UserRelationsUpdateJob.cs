using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Mutualify.Database;
using Mutualify.Jobs.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Jobs;

public class UserRelationsUpdateJob : IUserRelationsUpdateJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly IUsersService _usersService;
    private readonly IRelationsService _relationsService;
    private readonly ILogger<UserRelationsUpdateJob> _logger;

    private const int _interval = 3; // seconds

    private static bool _isRunning = false;
    private static DateTime _lastStartDate;

    public UserRelationsUpdateJob(IUsersService usersService, IRelationsService relationsService, ILogger<UserRelationsUpdateJob> logger, DatabaseContext databaseContext)
    {
        _usersService = usersService;
        _relationsService = relationsService;
        _logger = logger;
        _databaseContext = databaseContext;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 24)]
    public async Task Run(PerformContext context, CancellationToken token)
    {
        var jobId = context.BackgroundJob.Id;

        using var _ = _logger.BeginScope("UserRelationsUpdateJob");

        _logger.LogInformation("[{JobId}] Starting user relations update job...", jobId);

        if (_isRunning && _lastStartDate.AddDays(1) > DateTime.Now)
        {
            _logger.LogInformation("[{JobId}] User relations job is already running, abort!", jobId);
            return;
        }

        _isRunning = true;
        _lastStartDate = DateTime.Now;

        var userUpdateQueue = await _databaseContext.Tokens.AsNoTracking()
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken: token);

        for (var i = 0; i < userUpdateQueue.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            var userId = userUpdateQueue[i];
            var startTime = DateTime.Now;

            try
            {
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
                if (e.InnerException is HttpRequestException)
                {
                    // don't fail on HttpRequestExceptions, just keep going
                    continue;
                }

                _isRunning = false;

                throw;
            }
            catch (DbUpdateConcurrencyException) { } // don't fail on HttpRequestExceptions or DbUpdateConcurrencyException, just keep going
            catch (HttpRequestException) { }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[{JobId}] User relations update job has been cancelled!", jobId);

                _isRunning = false;
                return;
            }
            finally
            {
                var endTime = DateTime.Now;

                // yea its not really accurate but we don't need precision, just run it approximately every X seconds
                var elapsed = endTime - startTime;
                var timeout = elapsed.TotalSeconds < _interval ? _interval - (int) elapsed.TotalSeconds : 0;

                await Task.Delay(timeout * 1000, token);
            }
        }

        _isRunning = false;
        _logger.LogInformation("[{JobId}] Finished user relations update job", jobId);
    }
}
