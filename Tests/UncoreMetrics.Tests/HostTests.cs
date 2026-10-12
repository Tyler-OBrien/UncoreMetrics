using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sentry;
using Sentry.AspNetCore;
using UncoreMetrics.Steam_Collector;

namespace UncoreMetrics.Tests;

/// <summary>
///     Builds the real Steam Collector host, to catch DI/config wiring mistakes that only show up at startup.
/// </summary>
public class SteamCollectorHostTests
{
    private static string[] Args(string dsn) =>
    [
        "--Base:GameType=Rust",
        "--Base:PostgresConnectionString=Host=unused",
        $"--Base:SENTRY_DSN={dsn}"
    ];

    [Fact]
    public void SentryHub_ResolvesWithEmptyDsn()
    {
        // An empty DSN disables Sentry. This used to throw "You must supply a DSN" because the configured
        // options weren't the ones AddSentry actually reads.
        using var host = Program.BuildHost(Args(""));

        var hub = host.Services.GetRequiredService<IHub>();

        Assert.False(hub.IsEnabled);
    }

    [Fact]
    public void SentryOptions_UseDsnFromConfiguration()
    {
        using var host = Program.BuildHost(Args("https://key@example.invalid/1"));

        // Read the options without creating the hub, so nothing tries to talk to Sentry
        var options = host.Services.GetRequiredService<IOptions<SentryAspNetCoreOptions>>().Value;

        Assert.Equal("https://key@example.invalid/1", options.Dsn);
        Assert.Equal(1.0, options.TracesSampleRate);
    }
}

public class DiscordBotHostTests
{
    [Fact]
    public void SentryHub_ResolvesWithoutAnyDsnConfigured()
    {
        using var host = DiscordBot.Program.BuildHost(
        [
            "--DiscordBotConfig:DiscordToken=unused",
            "--DiscordBotConfig:PostgresConnectionString=Host=unused"
        ]);

        var hub = host.Services.GetRequiredService<IHub>();

        Assert.False(hub.IsEnabled);
    }
}
