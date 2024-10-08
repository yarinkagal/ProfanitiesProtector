using Azure;
using Azure.AI.ContentSafety;

namespace ProfanitiesProtector
{
    public class ContentSafetyHandler
    {
        private readonly ContentSafetyClient _contentSafetyClient;

        private const string ContentSafetyEndpoint = "https://profanitiesprotector-contentsafety.cognitiveservices.azure.com";
        private string ContentSafetyKey = Environment.GetEnvironmentVariable("ContentSafetyKey") ?? "5b0ab5b1d3e443cd9757ebd50f4dc589";

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

        public async Task<AnalyzeImageResult> AnalyzeImageAsync(string imageData)
        {
            ContentSafetyImageData image = new ContentSafetyImageData(new BinaryData(imageData));

            var request = new AnalyzeImageOptions(image);
            var analysisResult = await _contentSafetyClient.AnalyzeImageAsync(request);
            return analysisResult;
        }

    }
}