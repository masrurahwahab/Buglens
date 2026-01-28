using Buglens.Contract.IRepository;
using Buglens.Contract.IServices;
using BugLens.Data;
using Buglens.DTOs;
using Buglens.DTOs.Response;
using Buglens.Model;
using Microsoft.EntityFrameworkCore;

namespace Buglens.Service;

public class AnalysisService : IAnalysisService
{
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IAnalysisRepository analysisRepository,
        IGeminiService geminiService,
        ILogger<AnalysisService> logger)
    {
        _analysisRepository = analysisRepository;
        _geminiService = geminiService;
        _logger = logger;
    }

    
    public async Task<AnalysisResponse> CreateAnalysisAsync(AnalysisRequest request, string userId)
    {
        var analysis = new Analysis
        {
            UserId = userId,  
            Language = SanitizeInput(request.Language),
            ErrorLogs = SanitizeInput(request.ErrorLogs),
            SourceCode = SanitizeInput(request.SourceCode),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            
            var geminiResponse = await _geminiService.AnalyzeCodeAsync(
                analysis.Language,
                analysis.ErrorLogs,
                analysis.SourceCode
            );

            
            analysis.RootCause = SanitizeInput(geminiResponse.RootCause);
            analysis.Explanation = SanitizeInput(geminiResponse.Explanation);
            analysis.Fix = SanitizeInput(geminiResponse.SuggestedFix);
            analysis.CorrectedCode = SanitizeInput(geminiResponse.CorrectedCode);
            analysis.Success = true;
        }
        catch (Exception ex)
        {
            var gemini = await _geminiService.ListAvailableModelsAsync();
            _logger.LogError(ex, "Failed to analyze code with Gemini");
            
            analysis.Success = false;
            analysis.ErrorMessage = "Failed to analyze code. Please try again.";
            analysis.RootCause = "Analysis Failed";
            analysis.Explanation = $"Unable to complete analysis: {ex.Message}";
            analysis.Fix = "Please check your input and try again, or contact support if the issue persists.";
            analysis.CorrectedCode = analysis.SourceCode;
        }

        
        await _analysisRepository.AddAsync(analysis);

        return MapToResponse(analysis);
    }

    
    public async Task<AnalysisResponse?> GetAnalysisByIdAsync(int id, string userId)
    {
        var analysis = await _analysisRepository.GetByIdAsync(id);
        
     
        if (analysis == null || analysis.UserId != userId)
            return null;
            
        return MapToResponse(analysis);
    }

  
    public async Task<List<AnalysisResponse>> GetAnalysisHistoryAsync(string userId, int limit = 50)
    {
        var analyses = await _analysisRepository.GetRecentAnalysesByUserAsync(userId, limit);
        return analyses.Select(MapToResponse).ToList();
    }

   
    public async Task<List<AnalysisResponse>> GetAnalysesByLanguageAsync(string language, string userId)
    {
        var analyses = await _analysisRepository.GetAnalysesByLanguageAndUserAsync(language, userId);
        return analyses.Select(MapToResponse).ToList();
    }

    private string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

       
        return input
            .Replace("<script>", "")
            .Replace("</script>", "")
            .Replace("--", "")
            .Replace(";--", "")
            .Trim();
    }

    private AnalysisResponse MapToResponse(Analysis analysis)
    {
        return new AnalysisResponse
        {
            Id = analysis.Id,
            Language = analysis.Language,
            ErrorLogs = analysis.ErrorLogs,
            SourceCode = analysis.SourceCode,
            RootCause = analysis.RootCause,
            Explanation = analysis.Explanation,
            Fix = analysis.Fix,
            CorrectedCode = analysis.CorrectedCode,
            CreatedAt = analysis.CreatedAt,
            Success = analysis.Success,
            ErrorMessage = analysis.ErrorMessage
        };
    }
}