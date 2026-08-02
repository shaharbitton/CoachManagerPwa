using Postgrest.Attributes;
using Postgrest.Models;

namespace CoachManagerPwa.Models;

[Table("coach_assignment_contracts")]
public class CoachAssignmentContract : BaseModel
{
    [PrimaryKey("id", true)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("assign_id")]
    public string AssignId { get; set; } = string.Empty;

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [Column("coach_rate")]
    public decimal CoachRate { get; set; }

    [Column("allocated_hours")]
    public decimal? AllocatedHours { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Pending"; // Pending / Signed / Cancelled

    [Column("html_content")]
    public string HtmlContent { get; set; } = string.Empty;

    [Column("pdf_storage_path")]
    public string? PdfStoragePath { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("signed_at")]
    public DateTime? SignedAt { get; set; }

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }
}
