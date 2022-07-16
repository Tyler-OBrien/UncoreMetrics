using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Shared_Collectors.Models.Tools.Maxmind;

namespace Shared_Collectors.Tools.Maxmind;

public class MaxMindService : IGeoIPService
{
    private const string ASNFileName = "GeoLite2-ASN.mmdb";


    private const string CityFileName = "GeoLite2-City.mmdb";
    private readonly DatabaseReader? _dbReaderASN;

    private readonly DatabaseReader? _dbReaderCity;

    public MaxMindService()
    {
        if (File.Exists(ASNFileName) == false)
        {
#if DEBUG
            Console.WriteLine("Can't find ASN File for Maxmind...");
#endif
            _dbReaderASN = null;
        }
        else
        {
            _dbReaderASN = new DatabaseReader(ASNFileName);
        }

        if (File.Exists(CityFileName) == false)
            _dbReaderCity = null;
        else
            _dbReaderCity = new DatabaseReader(CityFileName);
    }


    public ValueTask<IPInformation> GetIpInformation(string address)
    {
        var newIpInformation = new IPInformation();

        var ASN = IPASNInformation(address);
        if (ASN != null)
        {
            newIpInformation.AutonomousSystemOrganization = ASN.AutonomousSystemOrganization;
            newIpInformation.AutonomousSystemNumber = ASN.AutonomousSystemNumber;
            newIpInformation.LargestNetworkCIDR = ASN.Network?.ToString();
        }

        var City = IPCityInformation(address);
        if (City != null)
        {
            newIpInformation.Continent = City.Continent.Name;
            newIpInformation.Latitude = City.Location.Latitude;
            newIpInformation.Longitude = City.Location.Longitude;
            newIpInformation.TimeZone = City.Location.TimeZone;
            newIpInformation.City = City.City.Name;
            newIpInformation.Country = City.Country.Name;
        }

        return ValueTask.FromResult(newIpInformation);
    }


    internal AsnResponse? IPASNInformation(string IP)
    {
        if (_dbReaderASN == null)
            return null;

        if (_dbReaderASN.TryAsn(IP, out var response)) return response;

        return null;
    }

    internal CityResponse? IPCityInformation(string IP)
    {
        if (_dbReaderCity == null)
            return null;

        if (_dbReaderCity.TryCity(IP, out var response)) return response;

        return null;
    }
}