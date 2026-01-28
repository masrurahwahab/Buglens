using Buglens.Contract.IRepository;
using BugLens.Data;
using Buglens.Model;
using Microsoft.EntityFrameworkCore;

namespace Buglens.Repository;

public class AnalysisRepository : Repository<Analysis>, IAnalysisRepository
{
    public AnalysisRepository(BugLensContext context) : base(context)
    {
    }

   
    public async Task<IEnumerable<Analysis>> GetRecentAnalysesAsync(int limit = 50)
    {
        return await _dbSet
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }


    public async Task<IEnumerable<Analysis>> GetRecentAnalysesByUserAsync(string userId, int limit = 50)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

   
    public async Task<IEnumerable<Analysis>> GetAnalysesByLanguageAsync(string language)
    {
        return await _dbSet
            .Where(a => a.Language == language)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

  
    public async Task<IEnumerable<Analysis>> GetAnalysesByLanguageAndUserAsync(string language, string userId)
    {
        return await _dbSet
            .Where(a => a.Language == language && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

  
    public async Task<IEnumerable<Analysis>> GetSuccessfulAnalysesAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.Success == true && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }


    public async Task<IEnumerable<Analysis>> GetFailedAnalysesAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.Success == false && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    
    public async Task<Analysis?> GetByIdAndUserAsync(int id, string userId)
    {
        return await _dbSet
            .Where(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();
    }

 
    public async Task<int> GetAnalysisCountByUserAsync(string userId)
    {
        return await _dbSet
            .CountAsync(a => a.UserId == userId);
    }

  
    public async Task<int> GetSuccessfulCountByUserAsync(string userId)
    {
        return await _dbSet
            .CountAsync(a => a.UserId == userId && a.Success == true);
    }
}