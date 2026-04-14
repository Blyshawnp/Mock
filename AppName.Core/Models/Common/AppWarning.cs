using AppName.Core.Models.Enums;

namespace AppName.Core.Models.Common;

public sealed class AppWarning
{
    public WarningSeverity Severity { get; init; } = WarningSeverity.Warning;

    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string RelatedSection { get; init; } = string.Empty;
}
