namespace CoachManagerPwa.Services;

/// <summary>
/// Thrown when attempting to modify a finalized time entry.
/// </summary>
public class ImmutableTimeEntryException : InvalidOperationException
{
    public ImmutableTimeEntryException(string entryId, string attemptedOperation)
        : base($"Cannot {attemptedOperation} time entry '{entryId}': Entry is finalized (Billed_Paid).")
    {
        EntryId = entryId;
        AttemptedOperation = attemptedOperation;
    }

    public string EntryId { get; }
    public string AttemptedOperation { get; }
}
