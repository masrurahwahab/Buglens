namespace Buglens.Model;

public class GeminiAnalysisResult
{
    public string RootCause { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string SuggestedFix { get; set; } = string.Empty;
    public string CorrectedCode { get; set; } = string.Empty;
}