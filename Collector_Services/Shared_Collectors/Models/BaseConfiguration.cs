namespace Shared_Collectors.Models;

public class BaseConfiguration
{
    public string PostgresConnectionString { get; set; }

    public string ClickhouseConnectionString { get; set; }

    public string? SteamAPIKey { get; set; }


    public int SecondsBetweenChecks { get; set; }

    public List<int> SecondsBetweenFailedChecks { get; set; }

    public int DaysUntilServerMarkedDead { get; set; }
}