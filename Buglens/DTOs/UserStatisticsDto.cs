namespace Buglens.Model;

public class UserStatisticsDto
{
    public OverviewStats Overview { get; set; }
    public List<LanguageUsageDto> LanguageUsage { get; set; }
    public List<TimelineDataDto> Timeline { get; set; }
    public List<ErrorTypeDto> CommonErrors { get; set; }
    public QuickStatsDto QuickStats { get; set; }
}
