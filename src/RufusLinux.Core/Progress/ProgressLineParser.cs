using System.Text.Json;

namespace RufusLinux.Core.Progress;

public static class ProgressLineParser
{
    public static ProgressEvent? TryParse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            string stage = root.TryGetProperty("stage", out var s) ? s.GetString() ?? "" : "";
            string status = root.TryGetProperty("status", out var st) ? st.GetString() ?? "" : "";
            double? percent = root.TryGetProperty("percent", out var p) && p.ValueKind == JsonValueKind.Number
                ? p.GetDouble()
                : null;
            string? detail = root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String
                ? d.GetString()
                : null;
            string? message = root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
                ? m.GetString()
                : null;

            return new ProgressEvent(stage, status, percent, detail, message);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
