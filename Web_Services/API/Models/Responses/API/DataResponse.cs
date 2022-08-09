using System.Text.Json.Serialization;

namespace API.Models.Responses.API;

/// <summary>
///     Default Rest API Data Response (i.e not an error)
/// </summary>
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