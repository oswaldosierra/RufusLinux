using System.Text.Json;
using System.Text.Json.Serialization;

namespace RufusLinux.Core.Jobs;

public static class WriteJobSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true,
    };

    public static string Serialize(WriteJobSpec spec) => JsonSerializer.Serialize(spec, Options);

    public static WriteJobSpec Deserialize(string json) =>
        JsonSerializer.Deserialize<WriteJobSpec>(json, Options)
        ?? throw new InvalidOperationException("Invalid job spec JSON.");
}
