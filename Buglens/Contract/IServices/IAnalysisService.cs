

using Buglens.DTOs;
using Buglens.DTOs.Response;

namespace Buglens.Contract.IServices;

public interface IAnalysisService
{
    Task<AnalysisResponse> CreateAnalysisAsync(AnalysisRequest request, string userId);
    Task<AnalysisResponse?> GetAnalysisByIdAsync(int id, string userId);
    Task<List<AnalysisResponse>> GetAnalysisHistoryAsync(string userId, int limit = 50);
    Task<List<AnalysisResponse>> GetAnalysesByLanguageAsync(string language, string userId);
}
