
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Mutualify.Database;
using Mutualify.Services.Interfaces;

namespace Mutualify.Jobs;
public interface IUserAllUpdateJob
{
    Task Run(PerformContext context, CancellationToken token);
}

/// <summary>
/// Apparently there are ~400k users in the database so updating them every day is not really possible
/// </summary>
public class UserAllUpdateJob : IUserAllUpdateJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly IUsersService _usersService;
    private readonly ILogger<UserUpdateJob> _logger;

    private const double _interval = 4; // seconds

    private static bool _isRunning = false;
    private static DateTime _lastStartDate;

    public UserAllUpdateJob(IUsersService usersService, ILogger<UserUpdateJob> logger, DatabaseContext databaseContext)
    {
        _usersService = usersService;
        _logger = logger;
        _databaseContext = databaseContext;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 24 * 14)]
    public async Task Run(PerformContext context, CancellationToken token)
    {
        var jobId = context.BackgroundJob.Id;

        using var _ = _logger.BeginScope("UserAllUpdateJob");

        _logger.LogInformation("[{JobId}] Starting all users update job...", jobId);

        if (_isRunning && _lastStartDate.AddDays(1) > DateTime.Now)
        {
            _logger.LogInformation("[{JobId}] Job is already running, abort!", jobId);
            return;
        }

        _isRunning = true;
        _lastStartDate = DateTime.Now;

        // since we might be running for a month shouldn't this be updated at some point?
        var userUpdateQueue = await _databaseContext.Users.AsNoTracking()
            .Where(x => x.UpdatedAt == null || x.UpdatedAt < DateTime.UtcNow.AddDays(-14))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken: token);

        for (var i = 0; i < userUpdateQueue.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            var userId = userUpdateQueue[i];
            var startTime = DateTime.Now;

            try
            {
#if !DEBUG
                if (i % 1000 == 0)
                {
#endif
                _logger.LogInformation("[{JobId}] ({Current}/{Total}) Updating {Id}...", jobId, i + 1,
                    userUpdateQueue.Count, userId);
#if !DEBUG
                }
#endif

                await _usersService.Update(userId, false);
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException)
                {
                    // don't fail on HttpRequestExceptions, just keep going
                    continue;
                }

                _logger.LogWarning(e, "[{JobId}] All users update job error occured!", jobId);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "[{JobId}] All users update job has been cancelled!", jobId);

                _isRunning = false;

                return;
            }
            catch (Exception ex)
            {
                // try to keep going
                _logger.LogWarning(ex, "[{JobId}] All users update job error occured!", jobId);
            }
            finally
            {
                var endTime = DateTime.Now;

                // yea its not really accurate but we don't need precision, just run it approximately every X seconds
                var elapsed = endTime - startTime;
                var timeout = elapsed.TotalSeconds < _interval ? _interval - (int) elapsed.TotalSeconds : 0;

                await Task.Delay((int) (timeout * 1000), token);
            }
        }

        _isRunning = false;
        _logger.LogInformation("[{JobId}] Finished all users update job", jobId);
    }
}
