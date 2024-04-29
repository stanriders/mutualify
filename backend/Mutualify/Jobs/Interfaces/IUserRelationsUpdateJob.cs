using Hangfire;
using Hangfire.Server;

namespace Mutualify.Jobs.Interfaces;

public interface IUserRelationsUpdateJob
{
    Task Run(PerformContext context, IJobCancellationToken token);
}
