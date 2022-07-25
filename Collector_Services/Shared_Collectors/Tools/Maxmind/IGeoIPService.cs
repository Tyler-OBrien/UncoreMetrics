using Shared_Collectors.Models.Tools.Maxmind;

namespace Shared_Collectors.Tools.Maxmind;

public interface IGeoIPService
{
    public ValueTask<IPInformation> GetIpInformation(string address);
}