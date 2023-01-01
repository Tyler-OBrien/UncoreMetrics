using System.ComponentModel.DataAnnotations.Schema;

namespace UncoreMetrics.Data;

public class ScrapeJob
{
    // Constructor for deseralization
    private ScrapeJob()
    {
    }

    public ScrapeJob(string gameType, string runType, string node, int runId, int progress, int totalDone,
        int totalCount, Guid runGuid, bool running, DateTime startedAt, DateTime lastUpdateUtc)
    {
        GameType = gameType;
        RunType = runType;
        Node = node;
        RunId = runId;
        Progress = progress;
        TotalDone = totalDone;
        TotalCount = totalCount;
        RunGuid = runGuid;
        Running = running;
        StartedAt = startedAt;
        LastUpdateUtc = lastUpdateUtc;
        InternalId = $"{GameType}-{Node}";
    }

    public void Copy(ScrapeJob newInfo)
    {
        GameType = newInfo.GameType;
        RunType = newInfo.RunType;
        Node = newInfo.Node;
        RunId = newInfo.RunId;
        Progress = newInfo.Progress;
        TotalDone = newInfo.TotalDone;
        TotalCount = newInfo.TotalCount;
        RunGuid = newInfo.RunGuid;
        Running = newInfo.Running;
        StartedAt = newInfo.StartedAt;
        LastUpdateUtc = newInfo.LastUpdateUtc;
        InternalId = $"{GameType}-{Node}";
    }

    [NotMapped] public string Name => $"{GameType}-{Node}-{RunId}";

    public string GameType { get; set; }

    public string RunType { get; set; }

    public string Node { get; set; }

    /// <summary>
    ///     Primary key, based off Node name and Game Type
    /// </summary>
    public string InternalId { get; set; }

    public int RunId { get; set; }

    public int Progress { get; set; }

    public int TotalDone { get; set; }

    public int TotalCount { get; set; }

    public Guid RunGuid { get; set; }

    public bool Running { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime LastUpdateUtc { get; set; }
}