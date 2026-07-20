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
        if (!_appState.IsAuthenticated || !_appState.IsAdmin) return;

        try
        {
            var coaches = await _data.GetCoachesAsync();
            var assignments = await _data.GetAssignmentsAsync();
            var timeEntries = await _data.GetTimeEntriesAsync();

            int pendingDocs = 0;
            int expiringDocs = 0;

            foreach (var coach in coaches)
            {
                try
                {
                    var docs = await _data.GetDocumentsByCoachAsync(coach.CoachId);
                    pendingDocs += docs.Count(d => d.Status == "Pending_Review");
                    expiringDocs += docs.Count(d => d.Status == "Approved" && d.ExpirationDate.HasValue && d.ExpirationDate.Value <= DateTime.Today.AddDays(30));
                }
                catch { }
            }

            var nearLimitAssignments = assignments.Count(a => a.Status == "Active" && a.AllocatedHours.HasValue && a.AllocatedHours > 0 &&
                timeEntries.Where(e => e.AssignId == a.AssignId).Sum(e => e.HoursReported) >= a.AllocatedHours.Value * 0.75m);

            _appState.SetNotificationCount(pendingDocs + expiringDocs + nearLimitAssignments);
        }
        catch { }
    }
}
