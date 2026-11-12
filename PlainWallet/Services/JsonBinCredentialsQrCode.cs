using System.Text.Json;

namespace PlainWallet.Services;

public sealed record JsonBinCredentials(string AccessKey, string BinId);

public static class JsonBinCredentialsQrCode
{
    private const string PayloadMarker = "PJB1";

    public static bool HasAllCredentials(string? accessKey, string? binId) =>
        !string.IsNullOrWhiteSpace(accessKey) &&
        !string.IsNullOrWhiteSpace(binId);

    public static string Encode(string accessKey, string binId)
    {
        if (!HasAllCredentials(accessKey, binId))
            throw new ArgumentException("The JSONBin access key and bin ID are required.");

        return JsonSerializer.Serialize(new[] { PayloadMarker, accessKey, binId });
    }

    public static bool TryDecode(string? value, out JsonBinCredentials? credentials)
    {
        credentials = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array ||
                root.GetArrayLength() != 3 ||
                root[0].ValueKind != JsonValueKind.String ||
                root[0].GetString() != PayloadMarker ||
                root[1].ValueKind != JsonValueKind.String ||
                root[2].ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var accessKey = root[1].GetString();
            var binId = root[2].GetString();
            if (!HasAllCredentials(accessKey, binId))
                return false;

            credentials = new JsonBinCredentials(accessKey!, binId!);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
