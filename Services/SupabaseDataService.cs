using CoachManagerPwa.Models;
using SupabaseClient = Supabase.Client;

namespace CoachManagerPwa.Services;

public class SupabaseDataService : IDataService
{
    private readonly SupabaseClient _client;
    private bool _initialized;

    public SupabaseDataService(SupabaseClient client)
    {
        _client = client;
    }

    public async Task RefreshAuthAsync()
    {
        _initialized = false;
        await EnsureInitializedAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await _client.InitializeAsync();
            _initialized = true;
        }

        // Inject the current JWT into Postgrest so RLS works
        var session = _client.Auth.CurrentSession;
        if (session?.AccessToken != null)
        {
            var headers = _client.Postgrest.Options.Headers;
            headers["Authorization"] = $"Bearer {session.AccessToken}";
        }
    }

    public SupabaseClient GetClient() => _client;

    // ===== Users =====

    public async Task<List<AppUser>> GetUsersAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<AppUser>().Get();
        return response.Models;
    }

    // ===== Coaches =====

    public async Task<List<Coach>> GetCoachesAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Coach>().Get();
        return response.Models;
    }

    public async Task<Coach?> GetCoachByIdAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Coach>()
            .Where(c => c.CoachId == coachId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<Coach> CreateCoachAsync(Coach coach)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Coach>().Insert(coach);
        return response.Models.First();
    }

    public async Task<Coach> UpdateCoachAsync(Coach coach)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Coach>().Update(coach);
        return response.Models.First();
    }

    public async Task DeleteCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        await _client.From<Coach>()
            .Where(c => c.CoachId == coachId)
            .Delete();
    }

    // ===== Coach Bank Details =====

    public async Task<CoachBankDetails?> GetBankDetailsByCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachBankDetails>()
            .Where(b => b.CoachId == coachId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<CoachBankDetails> UpsertBankDetailsAsync(CoachBankDetails details)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachBankDetails>().Upsert(details);
        return response.Models.First();
    }

    // ===== Coach Documents =====

    public async Task<List<CoachDocument>> GetDocumentsByCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachDocument>()
            .Where(d => d.CoachId == coachId)
            .Get();
        return response.Models;
    }

    public async Task<CoachDocument> CreateDocumentAsync(CoachDocument doc)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachDocument>().Insert(doc);
        return response.Models.First();
    }

    public async Task<CoachDocument> UpdateDocumentAsync(CoachDocument doc)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachDocument>().Update(doc);
        return response.Models.First();
    }

    public async Task DeleteDocumentAsync(string docId)
    {
        await EnsureInitializedAsync();
        await _client.From<CoachDocument>()
            .Where(d => d.DocId == docId)
            .Delete();
    }

    // ===== Coach Rates =====

    public async Task<List<CoachRate>> GetRatesByCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachRate>()
            .Where(r => r.CoachId == coachId)
            .Get();
        return response.Models;
    }

    public async Task<CoachRate?> GetActiveRateForAssignmentAsync(string coachId, string? assignId)
    {
        await EnsureInitializedAsync();
        var query = _client.From<CoachRate>()
            .Where(r => r.CoachId == coachId);

        if (assignId != null)
            query = query.Where(r => r.AssignId == assignId);

        var response = await query
            .Order("effective_from", Postgrest.Constants.Ordering.Descending)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<CoachRate> CreateRateAsync(CoachRate rate)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<CoachRate>().Insert(rate);
        return response.Models.First();
    }

    // ===== Clients =====

    public async Task<List<ClientOrg>> GetClientsAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientOrg>().Get();
        return response.Models;
    }

    public async Task<ClientOrg?> GetClientByIdAsync(string clientId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientOrg>()
            .Where(c => c.ClientId == clientId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<ClientOrg> CreateClientAsync(ClientOrg client)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientOrg>().Insert(client);
        return response.Models.First();
    }

    public async Task<ClientOrg> UpdateClientAsync(ClientOrg client)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientOrg>().Update(client);
        return response.Models.First();
    }

    // ===== Client Contracts =====

    public async Task<List<ClientContract>> GetContractsByClientAsync(string clientId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientContract>()
            .Where(c => c.ClientId == clientId)
            .Get();
        return response.Models;
    }

    public async Task<ClientContract> CreateContractAsync(ClientContract contract)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<ClientContract>().Insert(contract);
        return response.Models.First();
    }

    // ===== Assignments =====

    public async Task<List<Assignment>> GetAssignmentsAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Assignment>().Get();
        return response.Models;
    }

    public async Task<List<Assignment>> GetAssignmentsByCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Assignment>()
            .Where(a => a.CoachId == coachId)
            .Get();
        return response.Models;
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(string assignId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Assignment>()
            .Where(a => a.AssignId == assignId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Assignment>().Insert(assignment);
        return response.Models.First();
    }

    public async Task<Assignment> UpdateAssignmentAsync(Assignment assignment)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Assignment>().Update(assignment);
        return response.Models.First();
    }

    // ===== Time Entries =====

    public async Task<List<TimeEntry>> GetTimeEntriesAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TimeEntry>().Get();
        return response.Models;
    }

    public async Task<List<TimeEntry>> GetTimeEntriesByAssignmentAsync(string assignId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TimeEntry>()
            .Where(t => t.AssignId == assignId)
            .Get();
        return response.Models;
    }

    public async Task<TimeEntry?> GetTimeEntryByIdAsync(string entryId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TimeEntry>()
            .Where(t => t.EntryId == entryId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry entry)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TimeEntry>().Insert(entry);
        return response.Models.First();
    }

    public async Task<TimeEntry> UpdateTimeEntryAsync(TimeEntry entry)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TimeEntry>().Update(entry);
        return response.Models.First();
    }

    public async Task DeleteTimeEntryAsync(string entryId)
    {
        await EnsureInitializedAsync();
        await _client.From<TimeEntry>()
            .Where(t => t.EntryId == entryId)
            .Delete();
    }

    // ===== Training Resources =====

    public async Task<List<TrainingResource>> GetTrainingResourcesAsync()
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingResource>().Get();
        return response.Models;
    }

    public async Task<List<TrainingResource>> GetResourcesByCategoryAsync(string category)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingResource>()
            .Where(r => r.SkillCategory == category)
            .Get();
        return response.Models;
    }

    public async Task<TrainingResource> CreateResourceAsync(TrainingResource resource)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingResource>().Insert(resource);
        return response.Models.First();
    }

    // ===== Groups =====

    public async Task<List<TrainingGroup>> GetGroupsByCoachAsync(string coachId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingGroup>()
            .Where(g => g.CoachId == coachId)
            .Get();
        return response.Models;
    }

    public async Task<TrainingGroup?> GetGroupByIdAsync(string groupId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingGroup>()
            .Where(g => g.GroupId == groupId)
            .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<TrainingGroup> CreateGroupAsync(TrainingGroup group)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingGroup>().Insert(group);
        return response.Models.First();
    }

    public async Task<TrainingGroup> UpdateGroupAsync(TrainingGroup group)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<TrainingGroup>().Update(group);
        return response.Models.First();
    }

    public async Task DeleteGroupAsync(string groupId)
    {
        await EnsureInitializedAsync();
        await _client.From<TrainingGroup>()
            .Where(g => g.GroupId == groupId)
            .Delete();
    }

    // ===== Students =====

    public async Task<List<Student>> GetStudentsByGroupAsync(string groupId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Student>()
            .Where(s => s.GroupId == groupId)
            .Get();
        return response.Models;
    }

    public async Task<Student> CreateStudentAsync(Student student)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Student>().Insert(student);
        return response.Models.First();
    }

    public async Task<Student> UpdateStudentAsync(Student student)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<Student>().Update(student);
        return response.Models.First();
    }

    public async Task DeleteStudentAsync(string studentId)
    {
        await EnsureInitializedAsync();
        await _client.From<Student>()
            .Where(s => s.StudentId == studentId)
            .Delete();
    }

    // ===== Student Notes =====

    public async Task<List<StudentNote>> GetNotesByStudentAsync(string studentId)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<StudentNote>()
            .Where(n => n.StudentId == studentId)
            .Get();
        return response.Models;
    }

    public async Task<StudentNote> CreateNoteAsync(StudentNote note)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<StudentNote>().Insert(note);
        return response.Models.First();
    }

    public async Task<StudentNote> UpdateNoteAsync(StudentNote note)
    {
        await EnsureInitializedAsync();
        var response = await _client.From<StudentNote>().Update(note);
        return response.Models.First();
    }

    public async Task DeleteNoteAsync(string noteId)
    {
        await EnsureInitializedAsync();
        await _client.From<StudentNote>()
            .Where(n => n.NoteId == noteId)
            .Delete();
    }
}
