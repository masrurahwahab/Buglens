namespace Buglens.Model;
public class AnalysisStatistic
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Language { get; set; }
    public string ErrorType { get; set; }
    public bool IsResolved { get; set; }
    public double ResponseTimeSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}