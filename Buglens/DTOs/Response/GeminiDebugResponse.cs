namespace Buglens.DTOs.Response;

public class GeminiDebugResponse
{
    public string RootCause { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Fix { get; set; } = string.Empty;
    public string CorrectedCode { get; set; } = string.Empty;
}