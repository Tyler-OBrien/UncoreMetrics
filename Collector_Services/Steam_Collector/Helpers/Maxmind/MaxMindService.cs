using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Steam_Collector.Models.Tools.Maxmind;

namespace Steam_Collector.Helpers.Maxmind;

public class MaxMindService : IGeoIPService
{
    private const string ASNFileName = "GeoLite2-ASN.mmdb";


    private const string CityFileName = "GeoLite2-City.mmdb";
    private readonly DatabaseReader? _dbReaderASN;

    private readonly DatabaseReader? _dbReaderCity;
    private readonly ILogger _logger;

    public MaxMindService(ILogger<MaxMindService> logger)
    {
        _logger = logger;
        if (File.Exists(ASNFileName) == false)
        {
            _logger.LogWarning("Can't find ASN File for Maxmind Service... will not grab ASN Info");
            _dbReaderASN = null;
        }
        else
        {
            _dbReaderASN = new DatabaseReader(ASNFileName);
        }

        if (File.Exists(CityFileName) == false)
        {
            _logger.LogWarning("Can't find City File for Maxmind Service...will not grab Geo Info");
            _dbReaderCity = null;
        }
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

            newIpInformation.CountryCodeISO = City.Country.IsoCode;

            newIpInformation.CityCode = City.City.GeoNameId;
            newIpInformation.ContinentCode = City.Continent.Code;
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