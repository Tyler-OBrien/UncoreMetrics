using System.Net;
using System.Text.Json.Serialization;

namespace Ping_Collector_Probe.Models.API;

public class ErrorResponse : IResponse
{
    public ErrorResponse()
    {
    }

    public ErrorResponse(HttpStatusCode code, string message)
    {
        Error = new GenericError(code, message);
    }

    public ErrorResponse(int code, string message)
    {
        Error = new GenericError(code, message);
    }

    [JsonPropertyName("error")] public GenericError? Error { get; set; }
}

public class GenericError
{
    public GenericError()
    {
    }

    internal GenericError(HttpStatusCode code, string message)
    {
        Code = (int)code;
        Message = message;
    }

    internal GenericError(int code, string message)
    {
        Code = code;
        Message = message;
    }

    [JsonPropertyName("code")]
    // HTTP Status Code
    public int Code { get; set; }

    [JsonPropertyName("Message")] public string Message { get; set; }
}