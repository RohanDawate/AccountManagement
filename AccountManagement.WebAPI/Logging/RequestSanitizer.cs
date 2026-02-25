using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace AccountManagement.WebAPI.Logging
{
    public static class RequestSanitizer
    {
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization", "cookie", "set-cookie", "x-api-key", "x-amz-security-token",
        "password", "token", "secret", "creditCardNumber"
    };

        public static IDictionary<string, string> SanitizeHeaders(IHeaderDictionary headers) =>
            headers.ToDictionary(
                h => h.Key,
                h => SensitiveKeys.Contains(h.Key) ? "***REDACTED***" : h.Value.ToString()
            );

        public static IDictionary<string, string> SanitizeQuery(string? queryString)
        {
            if (string.IsNullOrEmpty(queryString)) return new Dictionary<string, string>();
            var parsed = QueryHelpers.ParseQuery(queryString);
            return parsed.ToDictionary(
                kvp => kvp.Key,
                kvp => SensitiveKeys.Contains(kvp.Key) ? "***REDACTED***" : kvp.Value.ToString()
            );
        }

        public static string SanitizeBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return body;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var sanitized = SanitizeElement(doc.RootElement);
                return JsonSerializer.Serialize(sanitized);
            }
            catch
            {
                return body; // non‑JSON bodies returned as-is
            }
        }

        private static object SanitizeElement(JsonElement element) =>
            element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(
                        prop => prop.Name,
                        prop => SensitiveKeys.Contains(prop.Name) ? "***REDACTED***" : SanitizeElement(prop.Value)
                    ),
                JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToList(),
                _ => element.ToString()
            };
    }

}
