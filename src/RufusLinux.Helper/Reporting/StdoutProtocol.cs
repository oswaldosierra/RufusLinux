using System.Text.Json;

namespace RufusLinux.Helper.Reporting;

public static class StdoutProtocol
{
    private static readonly object WriteLock = new();

    public static void Emit(string stage, string status, double? percent = null, string? detail = null, string? message = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["stage"] = stage,
            ["status"] = status,
        };
        if (percent is not null) payload["percent"] = percent;
        if (detail is not null) payload["detail"] = detail;
        if (message is not null) payload["message"] = message;

        string line = JsonSerializer.Serialize(payload);
        lock (WriteLock)
        {
            Console.WriteLine(line);
            Console.Out.Flush();
        }
    }

    public static void Running(string stage, double? percent = null, string? detail = null) =>
        Emit(stage, "running", percent, detail);

    public static void Done(string stage) => Emit(stage, "done");

    public static void Error(string stage, string message) => Emit(stage, "error", message: message);
}
