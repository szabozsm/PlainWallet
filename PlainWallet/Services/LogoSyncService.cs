using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace PlainWallet.Services;

public static class LogoSyncService
{
    private const string MockLogoUrl = "https://github.com/szabozsm/PlainWallet/raw/refs/heads/main/PlainWallet/Resources/Logos/logos.bin";
    private const string LocalLogoFileName = "logos.bin";

    public static async Task TryDownloadAsync()
    {
        if (SettingsStore.WifiOnlyForExtendsClass &&
            !Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
        {
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            using var request = new HttpRequestMessage(HttpMethod.Get, MockLogoUrl);
            var currentEtag = SettingsStore.LogoSyncEtag;
            if (!string.IsNullOrWhiteSpace(currentEtag))
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(currentEtag));
            }

            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return;
            }

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0)
            {
                return;
            }

            var targetFile = Path.Combine(FileSystem.AppDataDirectory, LocalLogoFileName);
            await File.WriteAllBytesAsync(targetFile, bytes);

            var newEtag = response.Headers.ETag?.Tag;
            if (string.IsNullOrWhiteSpace(newEtag))
            {
                SettingsStore.LogoSyncEtag = string.Empty;
                await SettingsStore.SaveAsync();
                LogosService.Reload();
                return;
            }

            SettingsStore.LogoSyncEtag = newEtag;
            await SettingsStore.SaveAsync();
            LogosService.Reload();
        }
        catch
        {
            // Ignore startup download failures and keep the bundled logo set.
        }
    }
}
