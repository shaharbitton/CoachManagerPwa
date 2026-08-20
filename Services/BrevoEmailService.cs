using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CoachManagerPwa.Services;

public class BrevoEmailService
{
    private readonly HttpClient _http;
    private readonly Supabase.Client _supabaseClient;
    private readonly string _functionsBaseUrl;

    public BrevoEmailService(HttpClient http, Supabase.Client supabaseClient, string supabaseUrl)
    {
        _http = http;
        _supabaseClient = supabaseClient;
        _functionsBaseUrl = supabaseUrl.TrimEnd('/') + "/functions/v1";
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string coachFirstName, string? accessToken = null)
    {
        var url = $"{_functionsBaseUrl}/send-welcome-email";
        var payload = JsonSerializer.Serialize(new { toEmail, coachFirstName });

        var token = accessToken ?? _supabaseClient.Auth.CurrentSession?.AccessToken;
        if (string.IsNullOrEmpty(token))
            throw new Exception("No active session — cannot call Edge Function");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(body);
    }
}
