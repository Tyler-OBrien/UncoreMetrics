﻿namespace UncoreMetrics.Steam_Collector.Helpers.ScrapeJobStatus;

public interface IScrapeJobStatusService
{
    public Task StartRun(int totalCount, string runType, CancellationToken token = default);

    public Task EndRun(string runType, CancellationToken token = default);

    public Task UpdateStatus(int progress, int totalDone, int totalCount, string runType,
        CancellationToken token = default);
}