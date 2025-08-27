using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Idempotency
{
    public static class IdempotencyHelpers
    {
        public static async Task<string> ComputeStableRequestHashAsync(HttpRequest request)
        {
            request.EnableBuffering();

            request.Body.Position = 0;
            using var sr = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var raw = await sr.ReadToEndAsync();
            request.Body.Position = 0;

            var normalized = NormalizeJsonOrRaw(raw);
            var toHash = $"{request.Method}\n{request.Path}\n{normalized}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(toHash));
            return Convert.ToHexString(bytes);
        }

        private static string NormalizeJsonOrRaw(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(body);
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
                {
                    WriteCanonical(doc.RootElement, writer);
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                // Not JSON → hash raw as-is
                return body.Trim();
            }
        }

        private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var prop in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(prop.Name);
                        WriteCanonical(prop.Value, writer);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                        WriteCanonical(item, writer);
                    writer.WriteEndArray();
                    break;

                default:
                    element.WriteTo(writer);
                    break;
            }
        }
    }
}