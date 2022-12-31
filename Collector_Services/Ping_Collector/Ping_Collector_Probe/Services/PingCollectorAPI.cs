using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ping_Collector_Probe.Models;
using System.Net.Http.Json;
using System.Text.Json;
using Ping_Collector_Probe.Extensions;
using Sentry;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;
using Ping_Collector_Probe.Models.API;

namespace Ping_Collector_Probe.Services
{
    public class PingCollectorAPI : IPingCollectorAPI
    {


        public static readonly JsonSerializerOptions JsonSerializerOptions =
            new() { AllowTrailingCommas = true, PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };


        private readonly HttpClient _httpClient;
        private readonly ProbeConfiguration _probeConfiguration;

        private readonly string API_PATH;


        public PingCollectorAPI(HttpClient client, IOptions<ProbeConfiguration> probeConfigurationOptions)
        {
            _httpClient = client;
            _probeConfiguration = probeConfigurationOptions.Value;
            API_PATH = _probeConfiguration.API_ENDPOINT;
            _httpClient.BaseAddress = new Uri(API_PATH);
            // For auth and such, i.e I use Cf Access w/ Bearer Tokens
            foreach (var header in _probeConfiguration.CUSTOM_REQUEST_HEADERS_FOR_API)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        public async Task<bool> RegisterLocation(Location location)
        {
            var returnedLocation = await _httpClient.PostAsJsonGetJsonAsync<Location, Response<Location>>($"RegisterLocation", location);
            if (returnedLocation is { Error: null })
                return true;
            return false;
        }

        public async Task<List<MiniServerDTO>> GetPingJobs(int locationId)
        {
            var returnedLocation = await _httpClient.GetFromJsonAsync<Response<List<MiniServerDTO>>>($"getpingjobs/{locationId}", JsonSerializerOptions);
            if (returnedLocation is not { Error: null })
                throw new Exception($"Error getting Ping Jobs: {returnedLocation.Error.Message}");
            foreach (var miniServerDto in returnedLocation?.Data)
            {
                miniServerDto.Address = IPAddress.Parse(miniServerDto.IpAddress);
            }
            return returnedLocation.Data;
        }

        public async Task<bool> SubmitScrapeJobUpdate(ScrapeJob newJobData)
        {
            var jobUpdateResult = await _httpClient.PostAsJsonGetJsonAsync<ScrapeJob, Response<GenericData>>( $"PingJobScrapeStatusUpdate", newJobData);
            if (jobUpdateResult is { Error: null })
                return true;
            return false;
        }

        public async Task<bool> SubmitPingJobs(PingJobCompleteDTO Data)
        {
            var submitResponse = await _httpClient.PostAsJsonGetJsonAsync<PingJobCompleteDTO, Response<GenericData>>( $"SubmitPingJobs", Data);
            if (submitResponse is { Error: null })
                return true;
            return false;
        }
    }
}
