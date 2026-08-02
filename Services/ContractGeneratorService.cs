using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public class ContractGeneratorService
{
    private readonly HttpClient _http;
    private string? _templateCache;

    // Base64-encoded business stamp image (embedded at compile time for security).
    // Replace this value with the actual base64 string of the stamp PNG.
    private static readonly Lazy<string> BusinessStampBase64 = new(() =>
    {
        var assembly = typeof(ContractGeneratorService).Assembly;
        using var stream = assembly.GetManifestResourceStream("CoachManagerPwa.Resources.business_stamp.jpeg");
        if (stream is null) return string.Empty;
        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        return Convert.ToBase64String(bytes);
    });

    public ContractGeneratorService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GenerateHtmlAsync(Assignment assignment, Coach coach, ClientOrg client, ClientContract? contract, decimal coachRate)
    {
        var template = await GetTemplateAsync();

        var html = template
            .Replace("{{FIRST_NAME}}", coach.FirstName)
            .Replace("{{LAST_NAME}}", coach.LastName)
            .Replace("{{NATIONAL_ID}}", coach.NationalId ?? "")
            .Replace("{{ADDRESS}}", coach.Address ?? "")
            .Replace("{{CONTRACT_DATE}}", DateTime.Today.ToString("dd/MM/yyyy"))
            .Replace("{{CLIENT_NAME}}", client.ClientName)
            .Replace("{{CLIENT_ADDRESS}}", client.City ?? "")
            .Replace("{{PERIOD_START}}", contract?.StartDate.ToString("dd/MM/yyyy") ?? "")
            .Replace("{{PERIOD_END}}", contract?.EndDate?.ToString("dd/MM/yyyy") ?? "פתוח")
            .Replace("{{COACH_RATE}}", coachRate.ToString("0.##"))
            .Replace("{{ALLOCATED_HOURS}}", assignment.AllocatedHours?.ToString("0.#") ?? "—")
            .Replace("{{SIGNATURE_DATE}}", $"<span id=\"signature-date\">{DateTime.Today:dd/MM/yyyy}</span>");

        // Embed business stamp image from embedded resource
        if (!string.IsNullOrEmpty(BusinessStampBase64.Value))
        {
            var stampImg = $"<img src=\"data:image/jpeg;base64,{BusinessStampBase64.Value}\" class=\"signature-image\" alt=\"חותמת העסק\" />";
            html = html.Replace("<!-- BUSINESS_STAMP_PLACEHOLDER -->", stampImg);
        }

        return html;
    }

    public string EmbedSignature(string html, string signatureDataUrl)
    {
        var imgTag = $"<img src=\"{signatureDataUrl}\" class=\"signature-image\" alt=\"חתימת המאמן\" />";
        html = html.Replace("<!-- COACH_SIGNATURE_PLACEHOLDER -->", imgTag);
        // Update signature date to actual signing moment
        var datePattern = "<span id=\"signature-date\">";
        var dateEndTag = "</span>";
        var startIdx = html.IndexOf(datePattern);
        if (startIdx >= 0)
        {
            var contentStart = startIdx + datePattern.Length;
            var endIdx = html.IndexOf(dateEndTag, contentStart);
            if (endIdx >= 0)
                html = string.Concat(html.AsSpan(0, contentStart), DateTime.Now.ToString("dd/MM/yyyy"), html.AsSpan(endIdx));
        }
        else
        {
            html = html.Replace("{{SIGNATURE_DATE}}", DateTime.Now.ToString("dd/MM/yyyy"));
        }
        return html;
    }

    private async Task<string> GetTemplateAsync()
    {
        if (_templateCache != null) return _templateCache;
        _templateCache = await _http.GetStringAsync("templates/contract_template.html");
        return _templateCache;
    }
}
