namespace Mutualify.Contracts;

public class StatsContract
{
    public int RegisteredCount { get; set; }
    public long RelationCount { get; set; }
    public long UserCount { get; set; }
    public int LastDayRegisteredCount { get; set; }
    public int EligibleForUpdateCount { get; set; }
}
