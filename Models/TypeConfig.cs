using Postgrest.Attributes;
using Postgrest.Models;

namespace CoachManagerPwa.Models;

/// <summary>
/// Admin-configurable lookup/type definition, keyed by Category.
/// Category values in use: "Document", "Contract" (mandatory doc/contract types),
/// "ResourceCategory" (Training Hub resource categories).
/// </summary>
[Table("type_configs")]
public class TypeConfig : BaseModel
{
    [PrimaryKey("config_id", true)]
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    [Column("category")]
    public string Category { get; set; } = "Document"; // Document / Contract / ResourceCategory

    [Column("doc_type")]
    public string DocType { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("is_mandatory")]
    public bool IsMandatory { get; set; }

    [Column("requires_expiration")]
    public bool RequiresExpiration { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
