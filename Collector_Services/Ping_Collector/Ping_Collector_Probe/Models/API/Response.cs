using System.Text.Json.Serialization;

namespace Ping_Collector_Probe.Models.API;

public class Response<T> : IResponse
{
    public Response()
    {
    }

    public Response(T data)
    {
        Data = data;
    }

    [JsonPropertyName("data")] public T? Data { get; set; }

    [JsonPropertyName("error")] public GenericError? Error { get; set; }
}