using BugLens.Data;
using Buglens.Model;
using Microsoft.EntityFrameworkCore;

namespace Buglens.Repository;

public class StatisticsRepository : IStatisticsRepository
    {
        private readonly BugLensContext _context;

        public StatisticsRepository(BugLensContext context)
        {
            _context = context;
        }

        public async Task<List<AnalysisStatistic>> GetUserStatisticsAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AnalysisStatistic>> GetUserStatisticsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAnalysisStatisticAsync(AnalysisStatistic statistic)
        {
            _context.AnalysisStatistics.Add(statistic);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnalysisResolutionAsync(int id, bool isResolved)
        {
            var statistic = await _context.AnalysisStatistics.FindAsync(id);
            if (statistic != null)
            {
                statistic.IsResolved = isResolved;
                statistic.ResolvedAt = isResolved ? DateTime.UtcNow : null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalAnalysesCountAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
                .CountAsync();
        }

        public async Task<int> GetResolvedBugsCountAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate && a.IsResolved)
                .CountAsync();
        }

        public async Task<double> GetAverageResponseTimeAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var stats = await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
                .ToListAsync();

            return stats.Any() ? stats.Average(a => a.ResponseTimeSeconds) : 0;
        }

        public async Task<List<LanguageUsageDto>> GetLanguageUsageAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var stats = await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
                .GroupBy(a => a.Language)
                .Select(g => new
                {
                    Language = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalCount = stats.Sum(s => s.Count);

            return stats.Select(s => new LanguageUsageDto
            {
                Language = s.Language,
                Count = s.Count,
                Percentage = totalCount > 0 ? Math.Round((double)s.Count / totalCount * 100, 2) : 0
            })
            .OrderByDescending(l => l.Count)
            .ToList();
        }

        public async Task<List<ErrorTypeDto>> GetCommonErrorsAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.AnalysisStatistics
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate && !string.IsNullOrEmpty(a.ErrorType))
                .GroupBy(a => a.ErrorType)
                .Select(g => new ErrorTypeDto
                {
                    ErrorType = g.Key,
                    Occurrences = g.Count(),
                    Severity = DetermineSeverity(g.Count())
                })
                .OrderByDescending(e => e.Occurrences)
                .Take(10)
                .ToListAsync();
        }

        private static string DetermineSeverity(int count)
        {
            if (count >= 20) return "high";
            if (count >= 10) return "medium";
            return "low";
        }
    }