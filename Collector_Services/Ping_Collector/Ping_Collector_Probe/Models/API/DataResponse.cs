using System.Net;
using System.Text.Json.Serialization;

namespace Ping_Collector_Probe.Models.API;

public class DataResponse<T> : IResponse
{
    public DataResponse()
    {
    }

    public DataResponse(T data)
    {
        Data = data;
    }

    [JsonPropertyName("data")] public T? Data { get; set; }
}

public class GenericData
{
    public GenericData()
    {
    }

    public GenericData(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public GenericData(HttpStatusCode code, string message)
    {
        Code = (int)code;
        Message = message;
    }

    [JsonPropertyName("code")]
    // HTTP Status Code
    public int Code { get; set; }

    [JsonPropertyName("Message")] public string Message { get; set; }
}