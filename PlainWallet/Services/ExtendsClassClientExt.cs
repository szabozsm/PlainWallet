namespace PlainWallet.Services
{
    public partial class ExtendsClassClient
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
        {
            // Add your custom header here
            request.Headers.Add("Security-key", "abcd123");
            request.Headers.Add("X-APIKey", "1897703b-2330-11f1-a204-0242ac110003"); // Add your API key here

        }
    }
}