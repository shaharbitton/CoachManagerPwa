using System.Net.Http.Json;
using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public interface ILocalityService
{
    Task InitializeAsync();
    IEnumerable<string> SearchLocalities(string query, int max = 20);
    IEnumerable<string> GetSubDistricts();
    string? GetSubDistrictForLocality(string localityName);
    bool IsLocalityInSubDistricts(string localityName, IEnumerable<string> subDistricts);
    List<Coach> GetRecommendedCoaches(string clientCity, List<Coach> coaches);
}

public class LocalityService : ILocalityService
{
    private readonly HttpClient _http;
    private List<Locality> _localities = new();
    private List<string> _subDistricts = new();
    private bool _initialized;

    public LocalityService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            _localities = await _http.GetFromJsonAsync<List<Locality>>("data/localities.json") ?? new();
            _subDistricts = _localities.Select(l => l.SubDistrict).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();
            _initialized = true;
        }
        catch
        {
            _localities = new();
            _subDistricts = new();
        }
    }

    public IEnumerable<string> SearchLocalities(string query, int max = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<string>();

        return _localities
            .Where(l => l.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Name)
            .Take(max);
    }

    public IEnumerable<string> GetSubDistricts() => _subDistricts;

    public string? GetSubDistrictForLocality(string localityName)
    {
        return _localities.FirstOrDefault(l => l.Name == localityName)?.SubDistrict;
    }

    public bool IsLocalityInSubDistricts(string localityName, IEnumerable<string> subDistricts)
    {
        var subDistrict = GetSubDistrictForLocality(localityName);
        if (subDistrict == null) return false;
        return subDistricts.Contains(subDistrict);
    }

    public List<Coach> GetRecommendedCoaches(string clientCity, List<Coach> coaches)
    {
        var clientSubDistrict = GetSubDistrictForLocality(clientCity);
        if (clientSubDistrict == null) return new();

        return coaches.Where(c =>
        {
            var areas = GetCoachAreas(c);
            return areas.Contains(clientSubDistrict);
        }).ToList();
    }

    private List<string> GetCoachAreas(Coach coach)
    {
        return coach.AvailabilityArea ?? new();
    }
}
