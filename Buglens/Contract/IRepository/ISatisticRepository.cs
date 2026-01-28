using Buglens.Model;

public interface IStatisticsRepository
{
    Task<List<AnalysisStatistic>> GetUserStatisticsAsync(string userId, int days);
    Task<List<AnalysisStatistic>> GetUserStatisticsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task AddAnalysisStatisticAsync(AnalysisStatistic statistic);
    Task UpdateAnalysisResolutionAsync(int id, bool isResolved);
    Task<int> GetTotalAnalysesCountAsync(string userId, int days);
    Task<int> GetResolvedBugsCountAsync(string userId, int days);
    Task<double> GetAverageResponseTimeAsync(string userId, int days);
    Task<List<LanguageUsageDto>> GetLanguageUsageAsync(string userId, int days);
    Task<List<ErrorTypeDto>> GetCommonErrorsAsync(string userId, int days);
}