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
                await RefreshCoachNotificationsAsync();
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

    private async Task RefreshCoachNotificationsAsync()
    {
        var coachId = _appState.CurrentUserId;
        int count = 0;

        // Coach's own expiring/rejected docs
        try
        {
            var docs = await _data.GetDocumentsByCoachAsync(coachId);
            count += docs.Count(d => d.Status == "Rejected");
            count += docs.Count(d => d.Status == "Approved" && d.ExpirationDate.HasValue && d.ExpirationDate.Value <= DateTime.Today.AddDays(30));
        }
        catch { }

        // Coach's assignments near quota
        try
        {
            var assignments = await _data.GetAssignmentsAsync();
            var myAssignments = assignments.Where(a => a.CoachId == coachId && a.Status == "Active" && a.AllocatedHours.HasValue && a.AllocatedHours > 0).ToList();
            if (myAssignments.Any())
            {
                var timeEntries = await _data.GetTimeEntriesAsync();
                count += myAssignments.Count(a =>
                    timeEntries.Where(e => e.AssignId == a.AssignId).Sum(e => e.HoursReported) >= a.AllocatedHours!.Value * 0.75m);
            }
        }
        catch { }

        _appState.SetNotificationCount(count);
    }
}
