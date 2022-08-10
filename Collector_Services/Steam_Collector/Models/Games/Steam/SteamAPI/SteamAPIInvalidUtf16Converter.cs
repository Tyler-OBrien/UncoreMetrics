using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

/// <summary>
/// This is a hack to get around the Steam API returning Invalid Utf16 chars for some foreign servers, we should be able to safely ignore those characters...
/// </summary>
public class SteamAPIInvalidUtf16Converter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return reader.GetString();
        }
        catch (InvalidOperationException)
        {
            var bytes = reader.ValueSpan;
            var sb = new StringBuilder(bytes.Length);
            foreach (var b in bytes)
                sb.Append(Convert.ToChar(b));
            return sb.ToString();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}