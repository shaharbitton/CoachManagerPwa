using System.Net.Http.Json;
using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public interface ILocalityService
{
    Task InitializeAsync();
    IEnumerable<string> SearchLocalities(string query, int max = 20);
    IEnumerable<string> GetSubDistricts();
    IEnumerable<string> GetDistricts();
    IEnumerable<string> GetSubDistricts(string? district);
    string? GetDistrictForSubDistrict(string subDistrict);
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

    // Israel's official Districts (Machoz) -> SubDistricts (Nafa) mapping.
    // The source dataset (localities.json) only exposes sub-district (Nafa) granularity,
    // so this mapping provides the higher-level District abstraction on top of it.
    private static readonly Dictionary<string, string> SubDistrictToDistrict = new()
    {
        ["ירושלים"] = "מחוז ירושלים",

        ["צפת"] = "מחוז הצפון",
        ["כנרת"] = "מחוז הצפון",
        ["עכו"] = "מחוז הצפון",
        ["גולן"] = "מחוז הצפון",
        ["נצרת"] = "מחוז הצפון",
        ["עפולה"] = "מחוז הצפון",

        ["חיפה"] = "מחוז חיפה",
        ["חדרה"] = "מחוז חיפה",

        ["השרון"] = "מחוז המרכז",
        ["פתח תקווה"] = "מחוז המרכז",
        ["רמלה"] = "מחוז המרכז",
        ["רחובות"] = "מחוז המרכז",

        ["תל אביב"] = "מחוז תל אביב",
        ["רמת גן"] = "מחוז תל אביב",
        ["חולון"] = "מחוז תל אביב",

        ["אשקלון"] = "מחוז הדרום",
        ["באר שבע"] = "מחוז הדרום",

        ["בית לחם"] = "יהודה ושומרון",
        ["ג'נין"] = "יהודה ושומרון",
        ["חברון"] = "יהודה ושומרון",
        ["טול כרם"] = "יהודה ושומרון",
        ["ירדן )יריחו("] = "יהודה ושומרון",
        ["ראמאללה"] = "יהודה ושומרון",
        ["שכם"] = "יהודה ושומרון",

        ["לא ידוע"] = "לא ידוע",
    };

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

    public IEnumerable<string> GetDistricts()
    {
        return _subDistricts
            .Select(GetDistrictForSubDistrict)
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList()!;
    }

    public IEnumerable<string> GetSubDistricts(string? district)
    {
        if (string.IsNullOrEmpty(district))
            return _subDistricts;

        return _subDistricts.Where(sd => GetDistrictForSubDistrict(sd) == district).ToList();
    }

    public string? GetDistrictForSubDistrict(string subDistrict)
    {
        return SubDistrictToDistrict.TryGetValue(subDistrict, out var district) ? district : null;
    }

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
