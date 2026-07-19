using Postgrest.Attributes;
using Postgrest.Models;

namespace CoachManagerPwa.Models;

// ===== שכבת זהויות והרשאות =====

[Table("users")]
public class AppUser : BaseModel
{
    [PrimaryKey("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("roles")]
public class Role : BaseModel
{
    [PrimaryKey("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }
}

[Table("user_roles")]
public class UserRole : BaseModel
{
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

// ===== שכבת פרופיל מאמן ומשאבי אנוש =====

[Table("coaches")]
public class Coach : BaseModel
{
    [PrimaryKey("coach_id", true)]
    public string CoachId { get; set; } = string.Empty;

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("national_id")]
    public string NationalId { get; set; } = string.Empty;

    [Column("birth_date")]
    public DateTime? BirthDate { get; set; }

    [Column("age")]
    public int? Age { get; set; }

    [Column("occupation")]
    public string? Occupation { get; set; }

    [Column("tax_status")]
    public string TaxStatus { get; set; } = "Employee";

    [Column("hard_skills")]
    public List<string>? HardSkills { get; set; } // text[] array in PostgreSQL

    [Column("availability_area")]
    public object? AvailabilityArea { get; set; } // JSONB object from PostgreSQL

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("coach_bank_details")]
public class CoachBankDetails : BaseModel
{
    [PrimaryKey("bank_info_id", true)]
    public string BankInfoId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("bank_name")]
    public string BankName { get; set; } = string.Empty;

    [Column("branch_number")]
    public string BranchNumber { get; set; } = string.Empty;

    [Column("account_number")]
    public string AccountNumber { get; set; } = string.Empty;

    [Column("beneficiary_name")]
    public string BeneficiaryName { get; set; } = string.Empty;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("coach_documents")]
public class CoachDocument : BaseModel
{
    [PrimaryKey("doc_id", true)]
    public string DocId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("doc_type")]
    public string DocType { get; set; } = string.Empty;

    [Column("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [Column("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Pending_Review";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("signed_documents")]
public class SignedDocument : BaseModel
{
    [PrimaryKey("signed_doc_id", true)]
    public string SignedDocId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("template_title")]
    public string TemplateTitle { get; set; } = string.Empty;

    [Column("signed_pdf_url")]
    public string SignedPdfUrl { get; set; } = string.Empty;

    [Column("signed_at")]
    public DateTime SignedAt { get; set; } = DateTime.UtcNow;
}

[Table("coach_evaluations")]
public class CoachEvaluation : BaseModel
{
    [PrimaryKey("eval_id", true)]
    public string EvalId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("reviewer_id")]
    public string ReviewerId { get; set; } = string.Empty;

    [Column("eval_date")]
    public DateTime EvalDate { get; set; } = DateTime.Today;

    [Column("score")]
    public int? Score { get; set; }

    [Column("strengths")]
    public string? Strengths { get; set; }

    [Column("weaknesses")]
    public string? Weaknesses { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}

[Table("coach_attributes")]
public class CoachAttribute : BaseModel
{
    [PrimaryKey("attribute_id", true)]
    public string AttributeId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [Column("added_by")]
    public string? AddedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ===== שכבת לקוחות, חוזים ושיבוצים =====

[Table("clients")]
public class ClientOrg : BaseModel
{
    [PrimaryKey("client_id", true)]
    public string ClientId { get; set; } = Guid.NewGuid().ToString();

    [Column("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [Column("tax_id")]
    public string? TaxId { get; set; }

    [Column("client_type")]
    public string ClientType { get; set; } = "School";

    [Column("address")]
    public string? Address { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("management_email")]
    public string? ManagementEmail { get; set; }

    [Column("accounting_email")]
    public string? AccountingEmail { get; set; }

    [Column("secretariat_email")]
    public string? SecretariatEmail { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("client_contracts")]
public class ClientContract : BaseModel
{
    [PrimaryKey("contract_id", true)]
    public string ContractId { get; set; } = Guid.NewGuid().ToString();

    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [Column("billing_rate_per_hour")]
    public decimal BillingRatePerHour { get; set; }

    [Column("work_scope_hours")]
    public decimal? WorkScopeHours { get; set; }

    [Column("payment_terms")]
    public string? PaymentTerms { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("assignments")]
public class Assignment : BaseModel
{
    [PrimaryKey("assign_id", true)]
    public string AssignId { get; set; } = Guid.NewGuid().ToString();

    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("contract_id")]
    public string? ContractId { get; set; }

    [Column("allocated_hours")]
    public decimal? AllocatedHours { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Active";

    [Column("satisfaction_score")]
    public int? SatisfactionScore { get; set; }

    [Column("rehire_recommended")]
    public bool? RehireRecommended { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("coach_rates")]
public class CoachRate : BaseModel
{
    [PrimaryKey("rate_id", true)]
    public string RateId { get; set; } = Guid.NewGuid().ToString();

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("assign_id")]
    public string? AssignId { get; set; }

    [Column("hourly_rate")]
    public decimal HourlyRate { get; set; }

    [Column("effective_from")]
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;

    [Column("effective_to")]
    public DateTime? EffectiveTo { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ===== שכבת תפעול שוטף =====

[Table("time_entries")]
public class TimeEntry : BaseModel
{
    [PrimaryKey("entry_id", true)]
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    [Column("assign_id")]
    public string AssignId { get; set; } = string.Empty;

    [Column("work_date")]
    public DateTime WorkDate { get; set; } = DateTime.Today;

    [Column("hours_reported")]
    public decimal HoursReported { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Pending_Signature";

    [Column("signature_url")]
    public string? SignatureUrl { get; set; }

    [Column("signer_name")]
    public string? SignerName { get; set; }

    [Column("signer_role")]
    public string? SignerRole { get; set; }

    [Column("gps_verified")]
    public bool GpsVerified { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("training_resources")]
public class TrainingResource : BaseModel
{
    [PrimaryKey("resource_id", true)]
    public string ResourceId { get; set; } = Guid.NewGuid().ToString();

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("skill_category")]
    public string SkillCategory { get; set; } = string.Empty;

    [Column("target_age_group")]
    public string? TargetAgeGroup { get; set; }

    [Column("attachment_url")]
    public string? AttachmentUrl { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ===== שכבת קבוצות ומשתתפים =====

[Table("groups")]
public class TrainingGroup : BaseModel
{
    [PrimaryKey("group_id", true)]
    public string GroupId { get; set; } = Guid.NewGuid().ToString();

    [Column("assign_id")]
    public string AssignId { get; set; } = string.Empty;

    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("group_name")]
    public string GroupName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("students")]
public class Student : BaseModel
{
    [PrimaryKey("student_id", true)]
    public string StudentId { get; set; } = Guid.NewGuid().ToString();

    [Column("group_id")]
    public string GroupId { get; set; } = string.Empty;

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("general_notes")]
    public string? GeneralNotes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("student_notes")]
public class StudentNote : BaseModel
{
    [PrimaryKey("note_id", true)]
    public string NoteId { get; set; } = Guid.NewGuid().ToString();

    [Column("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [Column("coach_id")]
    public string CoachId { get; set; } = string.Empty;

    [Column("lesson_date")]
    public DateTime LessonDate { get; set; } = DateTime.Today;

    [Column("lesson_topic")]
    public string? LessonTopic { get; set; }

    [Column("note_text")]
    public string NoteText { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}