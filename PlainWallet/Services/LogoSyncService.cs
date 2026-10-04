using System;
using System.IO;
using System.Net.Http;
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

            var bytes = await client.GetByteArrayAsync(MockLogoUrl);
            if (bytes.Length == 0)
            {
                return;
            }

            var targetFile = Path.Combine(FileSystem.AppDataDirectory, LocalLogoFileName);
            await File.WriteAllBytesAsync(targetFile, bytes);
            LogosService.Reload();
        }
        catch
        {
            // Ignore startup download failures and keep the bundled logo set.
        }
    }
}
