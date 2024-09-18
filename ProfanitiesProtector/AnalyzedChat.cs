using Azure.AI.ContentSafety;
namespace ProfanitiesProtector;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
public class AnalyzedChat
{
    public string ChatName { get; set; }
    public List<Message> DetectedMessages { get; set; }

    public List<TextAnalysisObject> CategoriesAnalysis { get; set; }
    [JsonIgnore]
    private ContentSafetyHandler _contentSafetyHandler { get; set; }

    public AnalyzedChat() { }
    public AnalyzedChat(string chatName)
    {
        ChatName = chatName;
        DetectedMessages = new List<Message>();
        _contentSafetyHandler = new ContentSafetyHandler();
        CategoriesAnalysis = new List<TextAnalysisObject>();
    }

    public bool HasProfanities()
    {
        return DetectedMessages.Count > 0;
    }

    public async Task<AnalyzeTextResult> AnalyzeText( string inputText)
    {
        var analysisResult = await _contentSafetyHandler.AnalyzeTextAsync(inputText);

        return analysisResult;
    }

    public async Task AnalyzeChatAsync(UnanalyzedChat chat)
    {
        AnalyzeTextResult analysisResult;
        AnalyzeImageResult analyzeImageResult;

        foreach (var message in chat.Messages)
        {
            analysisResult = await AnalyzeText(message.Content);

            if (!string.IsNullOrEmpty(message.ImageUrl))
            {
                analyzeImageResult = await _contentSafetyHandler.AnalyzeImageAsync(message.ImageUrl);

                if (IsOffensive(analyzeImageResult))
                {
                    DetectedMessages.Add(message);
                }
            }

            if (IsOffensive(analysisResult))
            {
                foreach (var categoriesAnalysis in analysisResult.CategoriesAnalysis)
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
                DetectedMessages.Add(message);
            }
        }
    }

    private bool IsOffensive(AnalyzeTextResult analysisResult)
    {
        return analysisResult.CategoriesAnalysis.Any(c => c.Severity >0);
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

