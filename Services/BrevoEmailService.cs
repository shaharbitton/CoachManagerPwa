using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CoachManagerPwa.Services;

public class BrevoEmailService
{
    private readonly HttpClient _http;
    private readonly string _supabaseUrl;
    private readonly string _supabaseAnonKey;

    public BrevoEmailService(HttpClient http, string supabaseUrl, string supabaseAnonKey)
    {
        _http = http;
        _supabaseUrl = supabaseUrl.TrimEnd('/');
        _supabaseAnonKey = supabaseAnonKey;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string coachFirstName)
    {
        var url = $"{_supabaseUrl}/functions/v1/send-welcome-email";
        var payload = JsonSerializer.Serialize(new { toEmail, coachFirstName });

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("apikey", _supabaseAnonKey);
        request.Headers.Add("Authorization", $"Bearer {_supabaseAnonKey}");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(body);
    }
}
