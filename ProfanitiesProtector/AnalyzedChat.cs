using Azure.AI.ContentSafety;
namespace ProfanitiesProtector;
using System.Linq;
using System.Text.Json.Serialization;
public class AnalyzedChat
{
    public string ChatName { get; set; }
    public List<string> DetectedMessagesIn { get; set; }
    public List<string> DetectedMessagesOut { get; set; }
    public List<string> DetectedImagesIn { get; set; }
    public List<string> DetectedImagesOut { get; set; }

    public List<TextAnalysisObject> CategoriesAnalysis { get; set; }
    [JsonIgnore]
    private ContentSafetyHandler _contentSafetyHandler { get; set; }

    public AnalyzedChat() { }
    public AnalyzedChat(string chatName)
    {
        ChatName = chatName;
        DetectedMessagesIn = new List<string>();
        DetectedMessagesOut = new List<string>();
        DetectedImagesIn = new List<string>();
        DetectedImagesOut = new List<string>();
        _contentSafetyHandler = new ContentSafetyHandler();
        CategoriesAnalysis = new List<TextAnalysisObject>();
    }

    public bool HasProfanities()
    {
        return DetectedMessagesIn.Any() ||
                DetectedMessagesOut.Any() ||
                DetectedImagesIn.Any() ||
                DetectedImagesOut.Any();
    }

    public async Task<AnalyzeTextResult> AnalyzeText( string inputText)
    {
        if(inputText.Length == 0)
        {
            return null;
        }
        var analysisResult = await _contentSafetyHandler.AnalyzeTextAsync(inputText);

        return analysisResult;
    }

    public void UpdateCategories(dynamic analyzeResult)
    {
        foreach (var categoriesAnalysis in analyzeResult.CategoriesAnalysis)
        {
            if (categoriesAnalysis != null)
            {
                var categoryName = categoriesAnalysis.Category.ToString();
                var severity = categoriesAnalysis.Severity ?? 0; // Default to 0 if null

                var categoryObject = CategoriesAnalysis.FirstOrDefault(c => c.Category == categoryName);

                if (categoryObject != null)
                {
                    categoryObject.Severity = Math.Max(categoryObject.Severity, severity);
                }
                else
                {
                    CategoriesAnalysis.Add(new TextAnalysisObject
                    {
                        Category = categoryName,
                        Severity = severity
                    });
                }
            }
        }
    }

    public async Task<List<string>> AnalyzeImagesAsync(List<string> images)
    {
        AnalyzeImageResult analyzeImageResult;
        var DetectedImages = new List<string>();
        foreach(var image in images)
        {
            analyzeImageResult = await _contentSafetyHandler.AnalyzeImageAsync(image);

            if (IsOffensive(analyzeImageResult))
            {
                UpdateCategories(analyzeImageResult);
                DetectedImages.Add(image);
            }
        }
        
        return DetectedImages;
    }

    public async Task<List<string>> AnalyzeMessagesAsync(List<string> messages)
    {
        AnalyzeTextResult analysisResult;
        AnalyzeImageResult analyzeImageResult;
        var DetectedMessages = new List<string>();

        foreach (var message in messages)
        {
            analysisResult = await AnalyzeText(message);

            if (IsOffensive(analysisResult))
            {
                UpdateCategories(analysisResult);
                DetectedMessages.Add(message);
            }
        }

        return DetectedMessages;
    }

    private bool IsOffensive(AnalyzeTextResult analysisResult)
    {
        return analysisResult!= null && analysisResult.CategoriesAnalysis.Any(c => c.Severity >0);
    }

    private bool IsOffensive(AnalyzeImageResult analysisResult)
    {
        return analysisResult.CategoriesAnalysis.Any(c => c.Severity > 0);
    }

    public class TextAnalysisObject
    {
        public string Category { get;  set; }
        public int Severity { get; set; }

        public TextAnalysisObject()
        {
        }

        public TextAnalysisObject(string category, int severity)
        {
            Category = category;
            Severity = severity;
        }
    }
}

