namespace PlainWallet.Services
{
    public partial class ExtendsClassClient
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
        {
            // Add your custom header here
            request.Headers.Add("security-key", SettingsStore.SecurityKey); // Add your security key here
            request.Headers.Add("api-key", SettingsStore.Apikey); // Add your API key here

        }
    }
}