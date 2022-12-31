using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ping_Collector_Probe.Models.API;

namespace Ping_Collector_Probe.Extensions
{
    public static class HttpExtensions
    {


        public static readonly JsonSerializerOptions JsonSerializerOptions =
            new() { AllowTrailingCommas = true, PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

        public static async Task<TValue?> PostAsJsonGetJsonAsync<TInput, TValue>(this HttpClient client, string? requestUri,
            TInput input)
            where TValue : class
        {
            var response = await client.PostAsJsonAsync(requestUri, input);
            response.ThrowForServerSideErrors();
            if (response.IsSuccessStatusCode)
            {
                var rawString = await response.Content.ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(rawString) == false)
                {
                    var output = JsonSerializer.Deserialize<TValue>(rawString, JsonSerializerOptions);

                    return output;
                }
            }

            return null;
        }


        public static void ThrowForServerSideErrors(this HttpResponseMessage msg)
        {
            msg.EnsureSuccessStatusCode();
        }
    }
}
