using System.Text.Json.Serialization;

namespace CoachManagerPwa.Models;

public class Locality
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("subDistrict")]
    public string SubDistrict { get; set; } = string.Empty;
}
