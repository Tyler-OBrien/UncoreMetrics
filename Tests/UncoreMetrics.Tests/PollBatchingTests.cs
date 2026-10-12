using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Game_Collectors;

namespace UncoreMetrics.Tests;

public class PollBatchingTests
{
    private static readonly DateTime Now = new(2026, 10, 11, 22, 0, 0, DateTimeKind.Utc);

    private static List<Server> DueServers(int count, TimeSpan overdueBy) =>
        Enumerable.Range(0, count).Select(_ => new Server { NextCheck = Now - overdueBy }).ToList();

    [Fact]
    public void FullBatch_Polls()
    {
        Assert.True(BaseResolver.ShouldPoll(DueServers(100, TimeSpan.FromSeconds(10)), 100, Now));
    }

    [Fact]
    public void SmallBatchNotWaitingLong_Waits()
    {
        Assert.False(BaseResolver.ShouldPoll(DueServers(87, TimeSpan.FromMinutes(1)), 100, Now));
    }

    [Fact]
    public void SmallBatchWaitingTooLong_Polls()
    {
        // e.g. Squad's 87 servers against a batch size of 100: previously never polled
        Assert.True(BaseResolver.ShouldPoll(DueServers(87, TimeSpan.FromMinutes(6)), 100, Now));
    }

    [Fact]
    public void OneLongWaitingServer_PollsTheWholeBatch()
    {
        var servers = DueServers(5, TimeSpan.FromSeconds(30));
        servers.Add(new Server { NextCheck = Now - TimeSpan.FromMinutes(10) });

        Assert.True(BaseResolver.ShouldPoll(servers, 1000, Now));
    }

    [Fact]
    public void NothingDue_DoesNotPoll()
    {
        Assert.False(BaseResolver.ShouldPoll([], 10, Now));
    }

    [Fact]
    public void ExactlyAtMaxDelay_StillWaits()
    {
        Assert.False(BaseResolver.ShouldPoll(DueServers(1, BaseResolver.MaxPollDelay), 10, Now));
    }
}
