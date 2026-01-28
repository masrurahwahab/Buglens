
using Buglens.Model;


namespace Buglens.Contract.IServices;


public interface IGeminiService
{
    Task<GeminiAnalysisResult> AnalyzeCodeAsync(string language, string errorLogs, string sourceCode);
    Task<string> ListAvailableModelsAsync();
}
