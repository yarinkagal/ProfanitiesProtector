using Azure;
using Azure.AI.ContentSafety;

namespace ProfanitiesProtector
{
    public class ContentSafetyHandler
    {
        private readonly ContentSafetyClient _contentSafetyClient;

        private const string ContentSafetyEndpoint = "https://profanitiesprotector-contentsafety.cognitiveservices.azure.com";
        private const string ContentSafetyKey = "5b0ab5b1d3e443cd9757ebd50f4dc589";

        public ContentSafetyHandler()
        {
            _contentSafetyClient = CreateContentSafetyClient();
        }

        public ContentSafetyClient CreateContentSafetyClient()
        {
            var endpoint = new Uri(ContentSafetyEndpoint);
            var credential = new AzureKeyCredential(ContentSafetyKey);
            return new ContentSafetyClient(endpoint, credential);
        }

        public async Task<AnalyzeTextResult> AnalyzeTextAsync(string inputText)
        {
            var analysisResult = await _contentSafetyClient.AnalyzeTextAsync(inputText);

            return analysisResult.Value;
        }

    }
}