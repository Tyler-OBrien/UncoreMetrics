using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI;

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
            Console.WriteLine($"Failed on {sb}, {BitConverter.ToString(reader.ValueSpan.ToArray())}");
            return sb.ToString();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}