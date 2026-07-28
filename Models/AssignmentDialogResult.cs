namespace CoachManagerPwa.Models;

public class AssignmentDialogResult
{
    public Assignment Assignment { get; set; } = new();
    public CoachRate? NewRate { get; set; }
}
