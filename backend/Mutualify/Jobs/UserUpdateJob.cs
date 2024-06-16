using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Mutualify.Database;
using Mutualify.Jobs.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Jobs;

public class UserUpdateJob : IUserUpdateJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly IUsersService _usersService;
    private readonly ILogger<UserUpdateJob> _logger;

    private const int _interval = 2; // seconds

    private static bool _isRunning = false;
    private static DateTime _lastStartDate;

    public UserUpdateJob(IUsersService usersService, ILogger<UserUpdateJob> logger, DatabaseContext databaseContext)
    {
        _usersService = usersService;
        _logger = logger;
        _databaseContext = databaseContext;
    }

    public async Task Run(PerformContext context, IJobCancellationToken token)
    {
        var jobId = context.BackgroundJob.Id;

        _logger.LogInformation("[{JobId}] Starting user update job...", jobId);

        if (_isRunning && _lastStartDate.AddDays(1.5) > DateTime.Now)
        {
            _logger.LogInformation("[{JobId}] Job is already running, abort!", jobId);
            return;
        }

        _isRunning = true;
        _lastStartDate = DateTime.Now;

        var userUpdateQueue = await _databaseContext.Users.AsNoTracking()
            .Where(x=> x.UpdatedAt == null || x.UpdatedAt < DateTime.UtcNow.AddDays(-1))
            .Select(x => x.Id)
            .ToListAsync();

        for (var i = 0; i < userUpdateQueue.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            var userId = userUpdateQueue[i];
            var startTime = DateTime.Now;

            try
            {
#if !DEBUG
                if (i % 100 == 0)
                {
#endif
                _logger.LogInformation("[{JobId}] ({Current}/{Total}) Updating {Id}...", jobId, i + 1,
                    userUpdateQueue.Count, userId);
#if !DEBUG
                }
#endif

                await _usersService.Update(userId);
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
                _logger.LogWarning("[{JobId}] User update job has been cancelled!", jobId);

                _isRunning = false;
                return;
            }
            finally
            {
                var endTime = DateTime.Now;

                // yea its not really accurate but we don't need precision, just run it approximately every X seconds
                var elapsed = endTime - startTime;
                var timeout = elapsed.TotalSeconds < _interval ? _interval - (int) elapsed.TotalSeconds : 0;

                await Task.Delay(timeout * 1000);
            }
        }

        _isRunning = false;
        _logger.LogInformation("[{JobId}] Finished user update job", jobId);
    }
}
