using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Networking;

namespace PlainWallet.Services;

public sealed class JsonBinClient
{
    private const string BaseUrl = "https://api.jsonbin.io/v3";
    private static readonly HttpClient HttpClient = new();

    public async Task<string> CreateBinAsync(object record, string accessKey)
    {
        EnsureWifiConnection();
        using var request = CreateRequest(HttpMethod.Post, $"{BaseUrl}/b", accessKey);
        request.Content = JsonContent.Create(record);

        using var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        if (!document.RootElement.TryGetProperty("metadata", out var metadata) ||
            !metadata.TryGetProperty("id", out var id) ||
            string.IsNullOrWhiteSpace(id.GetString()))
        {
            throw new InvalidDataException("JSONBin.io did not return a bin ID.");
        }

        return id.GetString()!;
    }

    public async Task UpdateBinAsync(object record, string binId, string accessKey)
    {
        EnsureWifiConnection();
        var uri = $"{BaseUrl}/b/{Uri.EscapeDataString(binId)}";
        using var request = CreateRequest(HttpMethod.Put, uri, accessKey);
        request.Content = JsonContent.Create(record);

        using var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetBinRecordAsync(string binId, string accessKey)
    {
        EnsureWifiConnection();
        var uri = $"{BaseUrl}/b/{Uri.EscapeDataString(binId)}";
        using var request = CreateRequest(HttpMethod.Get, uri, accessKey);
        using var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        if (!document.RootElement.TryGetProperty("record", out var record))
        {
            throw new InvalidDataException("JSONBin.io response did not contain a record.");
        }

        return record.GetRawText();
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, string accessKey)
    {
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new InvalidOperationException("Enter a JSONBin.io X-Access-Key before syncing.");
        }

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-Access-Key", accessKey);
        return request;
    }

    private static void EnsureWifiConnection()
    {
        if (SettingsStore.WifiOnlyForExtendsClass &&
            !Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
        {
            throw new WifiOnlySyncSkippedException();
        }
    }
}
