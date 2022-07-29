namespace Steam_Collector.Models.Tools.Maxmind;

public class IPInformation
{
    public string? AutonomousSystemOrganization { get; set; }

    public long? AutonomousSystemNumber { get; set; }

    public string? LargestNetworkCIDR { get; set; }

    public string? Continent { get; set; }

    public string? ContinentCode { get; set; }


    public string? Country { get; set; }

    public string? CountryCodeISO { get; set; }

    public string? City { get; set; }


    public long? CityCode { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? TimeZone { get; set; }
}