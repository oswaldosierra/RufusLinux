namespace RufusLinux.Core.Progress;

public sealed record ProgressEvent(
    string Stage,
    string Status,
    double? Percent = null,
    string? Detail = null,
    string? Message = null);
