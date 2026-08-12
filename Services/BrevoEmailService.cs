using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CoachManagerPwa.Services;

public class BrevoEmailService
{
    private readonly HttpClient _http;
    private const string ApiUrl = "https://api.brevo.com/v3/smtp/email";
    private const string ApiKey = "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";
    private const string FromEmail = "donot_replay_arcan@mop.co.il";
    private const string FromName = "Arcan Israel";

    public BrevoEmailService(HttpClient http)
    {
        _http = http;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string coachFirstName)
    {
        var htmlBody = $"""
            <div dir="rtl" style="font-family: Arial, sans-serif; line-height: 1.8; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                    <div style="text-align: center; margin-bottom: 20px;">
                        <img src="https://shaharbitton.github.io/CoachManagerPwa/icon-512.png" alt="Arcan Israel" width="80" height="80" style="display: block; margin: 0 auto 8px auto; border-radius: 50%;" />
                        <h2 style="color: #1a365d; margin: 0;">Arcan Israel</h2>
                        <p style="color: #666; margin: 4px 0;">מערכת ניהול מאמנים ושטח</p>
                    </div>
                    <hr style="border: none; border-top: 2px solid #1a365d; margin: 16px 0;" />
                    <p>שלום <strong>{coachFirstName}</strong>,</p>
                    <p>נוצר עבורך חשבון במערכת ניהול המאמנים של Arcan Israel.</p>
                    <div style="background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 16px; margin: 16px 0;">
                        <p style="margin: 4px 0;">🔗 <a href="https://shaharbitton.github.io/CoachManagerPwa/" style="color: #1a365d; font-weight: bold;">לכניסה למערכת</a></p>
                        <p style="margin: 4px 0;">📧 שם משתמש: <strong>{toEmail}</strong></p>
                        <p style="margin: 4px 0;">🔑 סיסמה ראשונית: <strong>Coach123!</strong></p>
                    </div>
                    <p>בכניסה הראשונה תתבקש/י לשנות סיסמה, להשלים פרטים אישיים ולהעלות אישורים כגון אישור משטרה.</p>
                    <p>בהצלחה!<br/><strong>צוות Arcan Israel</strong></p>
                </div>
            </div>
            """;

        var payload = new BrevoEmailPayload
        {
            Sender = new BrevoContact { Email = FromEmail, Name = FromName },
            To = [new BrevoContact { Email = toEmail, Name = coachFirstName }],
            Subject = "ברוך הבא למערכת Arcan Israel",
            HtmlContent = htmlBody
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("api-key", ApiKey);
        request.Content = JsonContent.Create(payload);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Brevo API error ({response.StatusCode}): {error}");
        }
    }

    private class BrevoEmailPayload
    {
        [JsonPropertyName("sender")]
        public BrevoContact Sender { get; set; } = new();

        [JsonPropertyName("to")]
        public List<BrevoContact> To { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = "";

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; } = "";
    }

    private class BrevoContact
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}
