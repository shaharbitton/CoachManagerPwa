namespace CoachManagerPwa.Services;

/// <summary>
/// A single coach-facing notification produced by <see cref="NotificationService"/>.
/// </summary>
public class CoachNotificationItem
{
    /// <summary>Stable unique key identifying this event instance (used for one-time acknowledgements).</summary>
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    /// <summary>MudBlazor color name (Error/Warning/Info/Primary/Success).</summary>
    public string Color { get; set; } = "Info";

    /// <summary>
    /// When true the notification stays until the underlying condition is resolved
    /// (e.g. contract signed, mandatory document uploaded). It cannot be dismissed.
    /// When false the coach may acknowledge it once to dismiss it permanently.
    /// </summary>
    public bool RequiresAction { get; set; }

    /// <summary>Optional deep-link the coach can follow to resolve the notification.</summary>
    public string? ActionHref { get; set; }
    public string? ActionText { get; set; }
}
