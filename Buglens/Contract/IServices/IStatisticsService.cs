using Buglens.Model;

namespace Buglens.Contract.IServices;

public interface IStatisticsService
{
    Task<UserStatisticsDto> GetUserStatisticsAsync(string userId, int days = 30);
    Task RecordAnalysisAsync(string userId, string language, string errorType, double responseTime);
    Task MarkAsResolvedAsync(int analysisId);
}