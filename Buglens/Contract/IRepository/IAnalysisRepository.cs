using Buglens.Model;

namespace Buglens.Contract.IRepository;

public interface IAnalysisRepository : IRepository<Analysis>
{
   
    Task<IEnumerable<Analysis>> GetRecentAnalysesAsync(int limit = 50);
    Task<IEnumerable<Analysis>> GetAnalysesByLanguageAsync(string language);
    
  
    Task<IEnumerable<Analysis>> GetRecentAnalysesByUserAsync(string userId, int limit = 50);
    Task<IEnumerable<Analysis>> GetAnalysesByLanguageAndUserAsync(string language, string userId);
    Task<IEnumerable<Analysis>> GetSuccessfulAnalysesAsync(string userId);
    Task<IEnumerable<Analysis>> GetFailedAnalysesAsync(string userId);
    Task<Analysis?> GetByIdAndUserAsync(int id, string userId);
    Task<int> GetAnalysisCountByUserAsync(string userId);
    Task<int> GetSuccessfulCountByUserAsync(string userId);
}