using Postgrest.Attributes;
using Postgrest.Models;

namespace CoachManagerPwa.Models;

[Table("tenant_subscriptions")]
public class TenantSubscription : BaseModel
{
    [PrimaryKey("subscription_id", true)]
    public string SubscriptionId { get; set; } = Guid.NewGuid().ToString();

    [Column("tenant_id")]
    public string TenantId { get; set; } = string.Empty;

    [Column("tier")]
    public int Tier { get; set; } = 0;

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("payment_ref")]
    public string? PaymentRef { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
