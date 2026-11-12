using System.Text.Json;

namespace PlainWallet.Services;

public sealed record ExtendsClassCredentials(string ApiKey, string SecurityKey, string BinId);

public static class ExtendsClassCredentialsQrCode
{
    private const string PayloadMarker = "PEC1";

    public static bool HasAllCredentials(string? apiKey, string? securityKey, string? binId) =>
        !string.IsNullOrWhiteSpace(apiKey) &&
        !string.IsNullOrWhiteSpace(securityKey) &&
        !string.IsNullOrWhiteSpace(binId);

    public static string Encode(string apiKey, string securityKey, string binId)
    {
        if (!HasAllCredentials(apiKey, securityKey, binId))
            throw new ArgumentException("All ExtendsClass credentials are required.");

        return JsonSerializer.Serialize(new[] { PayloadMarker, apiKey, securityKey, binId });
    }

    public static bool TryDecode(string? value, out ExtendsClassCredentials? credentials)
    {
        credentials = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array ||
                root.GetArrayLength() != 4 ||
                root[0].ValueKind != JsonValueKind.String ||
                root[0].GetString() != PayloadMarker ||
                root[1].ValueKind != JsonValueKind.String ||
                root[2].ValueKind != JsonValueKind.String ||
                root[3].ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var decodedApiKey = root[1].GetString();
            var decodedSecurityKey = root[2].GetString();
            var decodedBinId = root[3].GetString();
            if (decodedApiKey is null ||
                decodedSecurityKey is null ||
                decodedBinId is null ||
                !HasAllCredentials(decodedApiKey, decodedSecurityKey, decodedBinId))
                return false;

            credentials = new ExtendsClassCredentials(decodedApiKey, decodedSecurityKey, decodedBinId);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
