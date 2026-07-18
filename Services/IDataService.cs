using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public interface IDataService
{
    // Auth
    Task RefreshAuthAsync();

    // Users
    Task<List<AppUser>> GetUsersAsync();

    // Coaches
    Task<List<Coach>> GetCoachesAsync();
    Task<Coach?> GetCoachByIdAsync(string coachId);
    Task<Coach> CreateCoachAsync(Coach coach);
    Task<Coach> UpdateCoachAsync(Coach coach);
    Task DeleteCoachAsync(string coachId);

    // Coach Bank Details
    Task<CoachBankDetails?> GetBankDetailsByCoachAsync(string coachId);
    Task<CoachBankDetails> UpsertBankDetailsAsync(CoachBankDetails details);

    // Coach Documents
    Task<List<CoachDocument>> GetDocumentsByCoachAsync(string coachId);
    Task<CoachDocument> CreateDocumentAsync(CoachDocument doc);
    Task<CoachDocument> UpdateDocumentAsync(CoachDocument doc);
    Task DeleteDocumentAsync(string docId);

    // Coach Rates
    Task<List<CoachRate>> GetRatesByCoachAsync(string coachId);
    Task<CoachRate?> GetActiveRateForAssignmentAsync(string coachId, string? assignId);
    Task<CoachRate> CreateRateAsync(CoachRate rate);

    // Clients
    Task<List<ClientOrg>> GetClientsAsync();
    Task<ClientOrg?> GetClientByIdAsync(string clientId);
    Task<ClientOrg> CreateClientAsync(ClientOrg client);
    Task<ClientOrg> UpdateClientAsync(ClientOrg client);

    // Client Contracts
    Task<List<ClientContract>> GetContractsByClientAsync(string clientId);
    Task<ClientContract> CreateContractAsync(ClientContract contract);

    // Assignments
    Task<List<Assignment>> GetAssignmentsAsync();
    Task<List<Assignment>> GetAssignmentsByCoachAsync(string coachId);
    Task<Assignment?> GetAssignmentByIdAsync(string assignId);
    Task<Assignment> CreateAssignmentAsync(Assignment assignment);
    Task<Assignment> UpdateAssignmentAsync(Assignment assignment);

    // Time Entries
    Task<List<TimeEntry>> GetTimeEntriesAsync();
    Task<List<TimeEntry>> GetTimeEntriesByAssignmentAsync(string assignId);
    Task<TimeEntry?> GetTimeEntryByIdAsync(string entryId);
    Task<TimeEntry> CreateTimeEntryAsync(TimeEntry entry);
    Task<TimeEntry> UpdateTimeEntryAsync(TimeEntry entry);
    Task DeleteTimeEntryAsync(string entryId);

    // Training Resources
    Task<List<TrainingResource>> GetTrainingResourcesAsync();
    Task<List<TrainingResource>> GetResourcesByCategoryAsync(string category);
    Task<TrainingResource> CreateResourceAsync(TrainingResource resource);

    // Groups
    Task<List<TrainingGroup>> GetGroupsByCoachAsync(string coachId);
    Task<TrainingGroup?> GetGroupByIdAsync(string groupId);
    Task<TrainingGroup> CreateGroupAsync(TrainingGroup group);
    Task<TrainingGroup> UpdateGroupAsync(TrainingGroup group);
    Task DeleteGroupAsync(string groupId);

    // Students
    Task<List<Student>> GetStudentsByGroupAsync(string groupId);
    Task<Student> CreateStudentAsync(Student student);
    Task<Student> UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(string studentId);

    // Student Notes
    Task<List<StudentNote>> GetNotesByStudentAsync(string studentId);
    Task<StudentNote> CreateNoteAsync(StudentNote note);
    Task<StudentNote> UpdateNoteAsync(StudentNote note);
    Task DeleteNoteAsync(string noteId);
}
