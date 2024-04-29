﻿namespace Mutualify.Contracts;

public class StatsContract
{
    public int RegisteredCount { get; set; }
    public long RelationCount { get; set; }
    public int LastDayRegisteredCount { get; set; }
    public int EligibleForUpdateCount { get; set; }
    public int EligibleForUserUpdateCount { get; set; }
}
