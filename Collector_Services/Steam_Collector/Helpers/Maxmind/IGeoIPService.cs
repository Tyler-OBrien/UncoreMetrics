using UncoreMetrics.Steam_Collector.Models.Tools.Maxmind;

namespace UncoreMetrics.Steam_Collector.Helpers.Maxmind;

public interface IGeoIPService
{
    public ValueTask<IPInformation> GetIpInformation(string address);
}