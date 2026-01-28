namespace Buglens.DTOs.Response;

public class AnalysisResponse
{
    public int Id { get; set; }
    public string Language { get; set; } = string.Empty;
    public string ErrorLogs { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Fix { get; set; } = string.Empty;
    public string CorrectedCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}