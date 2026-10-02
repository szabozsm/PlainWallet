namespace PlainWallet.Services
{
    public partial class ExtendsClassClient
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
        {
            if (SettingsStore.WifiOnlyForExtendsClass &&
                !Microsoft.Maui.Networking.Connectivity.Current.ConnectionProfiles.Contains(
                    Microsoft.Maui.Networking.ConnectionProfile.WiFi))
            {
                throw new WifiOnlySyncSkippedException();
            }

            // Add your custom header here
            request.Headers.Add("security-key", SettingsStore.SecurityKey); // Add your security key here
            request.Headers.Add("api-key", SettingsStore.Apikey); // Add your API key here

        }
    }

    internal sealed class WifiOnlySyncSkippedException : Exception
    {
    }
}