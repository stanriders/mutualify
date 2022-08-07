using Hangfire;
using Hangfire.Server;

namespace Mutualify.Jobs.Interfaces;

public interface IUserUpdateJob
{
    Task Run(PerformContext context, IJobCancellationToken token);
}
