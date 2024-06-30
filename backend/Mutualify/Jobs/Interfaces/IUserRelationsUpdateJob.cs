using Hangfire.Server;

namespace Mutualify.Jobs.Interfaces;

public interface IUserRelationsUpdateJob
{
    Task Run(PerformContext context, CancellationToken token);
}
