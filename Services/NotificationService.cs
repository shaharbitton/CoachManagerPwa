using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public class NotificationService
{
    private readonly IDataService _data;
    private readonly AppState _appState;

    public NotificationService(IDataService data, AppState appState)
    {
        _data = data;
        _appState = appState;
    }

    // Fallback config used when the admin hasn't configured any document types yet.
    private static readonly (string Type, string Name, bool Mandatory)[] DefaultDocTypes =
    [
        ("National_ID_Card", "תעודת זהות", true),
        ("Police_Clearance", "אישור משטרה", true),
        ("Certification", "תעודת הסמכה", true),
        ("Tax_Withholding", "ניכוי מס במקור", true),
        ("Bank_Confirmation", "אישור ניהול חשבון", true),
        ("Recommendations", "המלצות", false),
    ];

    public async Task RefreshNotificationCountAsync()
    {
        if (!_appState.IsAuthenticated) return;

        try
        {
            if (_appState.IsAdmin)
            {
                await RefreshAdminNotificationsAsync();
            }
            else
            {
                var items = await GetCoachNotificationsAsync();
                _appState.SetNotificationCount(items.Count);
            }
        }
        catch { }
    }

    private async Task RefreshAdminNotificationsAsync()
    {
        var coaches = await _data.GetCoachesAsync();
        var assignments = await _data.GetAssignmentsAsync();
        var timeEntries = await _data.GetTimeEntriesAsync();

        int pendingDocs = 0;
        int expiringDocs = 0;

        var docTasks = coaches.Select(c => _data.GetDocumentsByCoachAsync(c.CoachId));
        var allDocs = await Task.WhenAll(docTasks);
        foreach (var docs in allDocs)
        {
            pendingDocs += docs.Count(d => d.Status == "Pending_Review");
            expiringDocs += docs.Count(d => d.Status == "Approved" && d.ExpirationDate.HasValue && d.ExpirationDate.Value <= DateTime.Today.AddDays(30));
        }

        var nearLimitAssignments = assignments.Count(a => a.Status == "Active" && a.AllocatedHours.HasValue && a.AllocatedHours > 0 &&
            timeEntries.Where(e => e.AssignId == a.AssignId).Sum(e => e.HoursReported) >= a.AllocatedHours.Value * 0.75m);

        _appState.SetNotificationCount(pendingDocs + expiringDocs + nearLimitAssignments);
    }

    /// <summary>
    /// Builds the full, rule-based list of active notifications for the current coach.
    /// One-time notifications already acknowledged by the coach are filtered out.
    /// </summary>
    public async Task<List<CoachNotificationItem>> GetCoachNotificationsAsync()
    {
        var coachId = _appState.CurrentUserId;
        var items = new List<CoachNotificationItem>();

        // Resolve document type configuration (mandatory vs optional).
        List<(string Type, string Name, bool Mandatory)> docTypes;
        try
        {
            var configs = await _data.GetDocumentTypeConfigsAsync();
            var docConfigs = configs.Where(c => c.IsActive && c.Category == "Document").OrderBy(c => c.SortOrder).ToList();
            docTypes = docConfigs.Count > 0
                ? docConfigs.Select(c => (c.DocType, c.DisplayName, c.IsMandatory)).ToList()
                : DefaultDocTypes.ToList();
        }
        catch
        {
            docTypes = DefaultDocTypes.ToList();
        }

        // Acknowledged one-time notifications.
        HashSet<string> acked;
        try
        {
            var acks = await _data.GetNotificationAcksByCoachAsync(coachId);
            acked = acks.Select(a => a.NotificationKey).ToHashSet();
        }
        catch { acked = new HashSet<string>(); }

        // ===== Contracts pending signature (persistent until signed) =====
        try
        {
            var coachContracts = await _data.GetContractsByCoachAsync(coachId);
            foreach (var c in coachContracts.Where(c => c.Status == "Pending"))
            {
                items.Add(new CoachNotificationItem
                {
                    Key = $"contract:{c.Id}",
                    Title = "חוזה ממתין לחתימה",
                    Message = $"חוזה מ-{c.CreatedAt:dd/MM/yyyy} ממתין לחתימתך.",
                    Icon = "Draw",
                    Color = "Primary",
                    RequiresAction = true,
                    ActionHref = $"coach/sign-contract/{c.Id}",
                    ActionText = "חתום"
                });
            }
        }
        catch { }

        // ===== Documents =====
        try
        {
            var docs = await _data.GetDocumentsByCoachAsync(coachId);

            foreach (var (type, name, mandatory) in docTypes)
            {
                var ofType = docs.Where(d => d.DocType == type).ToList();

                // Rejected documents: one-time acknowledgement per document instance.
                foreach (var rejected in ofType.Where(d => d.Status == "Rejected"))
                {
                    var key = $"doc-rejected:{rejected.DocId}";
                    if (!acked.Contains(key))
                    {
                        items.Add(new CoachNotificationItem
                        {
                            Key = key,
                            Title = "מסמך נדחה",
                            Message = $"המסמך '{name}' נדחה. יש להעלות מסמך חדש מסוג זה.",
                            Icon = "Cancel",
                            Color = "Error",
                            RequiresAction = false,
                            ActionHref = "coach/documents",
                            ActionText = "העלה מחדש"
                        });
                    }
                }

                // A document is considered "present" only if approved/pending and not expired.
                bool present = ofType.Any(d =>
                    (d.Status == "Approved" || d.Status == "Pending_Review") &&
                    (!d.ExpirationDate.HasValue || d.ExpirationDate.Value >= DateTime.Today));

                if (mandatory)
                {
                    // Expired mandatory document -> one-time notice that it expired.
                    // The persistent "missing document" notification below keeps nagging until a valid one is uploaded.
                    var expired = ofType.FirstOrDefault(d => d.Status == "Approved" && d.ExpirationDate.HasValue && d.ExpirationDate.Value < DateTime.Today);
                    if (expired is not null)
                    {
                        var key = $"doc-expired:{expired.DocId}:{expired.ExpirationDate:yyyyMMdd}";
                        if (!acked.Contains(key))
                        {
                            items.Add(new CoachNotificationItem
                            {
                                Key = key,
                                Title = "מסמך חובה פג תוקף",
                                Message = $"המסמך '{name}' פג תוקף. יש להעלות מסמך בתוקף מסוג זה.",
                                Icon = "EventBusy",
                                Color = "Error",
                                RequiresAction = false,
                                ActionHref = "coach/documents",
                                ActionText = "העלה מסמך"
                            });
                        }
                    }

                    // Missing mandatory document -> persistent until uploaded.
                    if (!present)
                    {
                        items.Add(new CoachNotificationItem
                        {
                            Key = $"doc-missing:{type}",
                            Title = "חסר מסמך חובה",
                            Message = $"חסר המסמך '{name}'. יש להעלות מסמך מסוג זה.",
                            Icon = "UploadFile",
                            Color = "Warning",
                            RequiresAction = true,
                            ActionHref = "coach/documents",
                            ActionText = "העלה מסמך"
                        });
                    }
                }

                // Expiring soon (approved, within 30 days) -> one-time acknowledgeable warning.
                var expiring = ofType.FirstOrDefault(d => d.Status == "Approved" && d.ExpirationDate.HasValue &&
                    d.ExpirationDate.Value >= DateTime.Today && d.ExpirationDate.Value <= DateTime.Today.AddDays(30));
                if (expiring is not null)
                {
                    var days = (expiring.ExpirationDate!.Value - DateTime.Today).Days;
                    var key = $"doc-expiring:{expiring.DocId}:{expiring.ExpirationDate:yyyyMMdd}";
                    if (!acked.Contains(key))
                    {
                        items.Add(new CoachNotificationItem
                        {
                            Key = key,
                            Title = "מסמך עומד לפוג",
                            Message = $"המסמך '{name}' יפוג בעוד {days} ימים.",
                            Icon = "Schedule",
                            Color = "Warning",
                            RequiresAction = false,
                            ActionHref = "coach/documents",
                            ActionText = "צפה"
                        });
                    }
                }
            }
        }
        catch { }

        // ===== Assignments near hour quota (one-time acknowledgeable) =====
        try
        {
            var assignments = await _data.GetAssignmentsAsync();
            var myAssignments = assignments.Where(a => a.CoachId == coachId && a.Status == "Active" && a.AllocatedHours.HasValue && a.AllocatedHours > 0).ToList();
            if (myAssignments.Any())
            {
                var timeEntries = await _data.GetTimeEntriesAsync();
                var clients = await _data.GetClientsAsync();
                foreach (var a in myAssignments)
                {
                    var used = timeEntries.Where(e => e.AssignId == a.AssignId).Sum(e => e.HoursReported);
                    var pct = (double)(used / a.AllocatedHours!.Value * 100);
                    if (pct >= 75)
                    {
                        var bucket = pct >= 100 ? "100" : "75";
                        var key = $"assign-quota:{a.AssignId}:{bucket}";
                        if (!acked.Contains(key))
                        {
                            var clientName = clients.FirstOrDefault(c => c.ClientId == a.ClientId)?.ClientName ?? "לקוח";
                            items.Add(new CoachNotificationItem
                            {
                                Key = key,
                                Title = "שיבוץ קרוב למגבלת שעות",
                                Message = $"השיבוץ עבור {clientName} הגיע ל-{pct:0}% מהשעות שהוקצו.",
                                Icon = "HourglassBottom",
                                Color = pct >= 100 ? "Error" : "Warning",
                                RequiresAction = false
                            });
                        }
                    }
                }
            }
        }
        catch { }

        return items;
    }

    /// <summary>
    /// Marks a one-time notification as acknowledged so it will not be shown again.
    /// </summary>
    public async Task AcknowledgeAsync(string notificationKey)
    {
        try
        {
            await _data.CreateNotificationAckAsync(new NotificationAck
            {
                CoachId = _appState.CurrentUserId,
                NotificationKey = notificationKey
            });
        }
        catch { }
        await RefreshNotificationCountAsync();
    }
}
