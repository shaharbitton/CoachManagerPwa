using Postgrest.Attributes;
using Postgrest.Models;

namespace CoachManagerPwa.Models;

/// <summary>
/// Records that a coach acknowledged a one-time notification so it is not shown again.
/// NotificationKey uniquely identifies the event instance (e.g. "rejected:{docId}").
/// </summary>
[Table("notification_acknowledgements")]
public class NotificationAck : BaseModel
{
    [PrimaryKey("ack_id", true)]
    public string AckId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("notification_key")]
    public string NotificationKey { get; set; } = string.Empty;

    [Column("acknowledged_at")]
    public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
}
