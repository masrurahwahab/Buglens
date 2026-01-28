using System.ComponentModel.DataAnnotations;

namespace Buglens.DTOs;

public class AnalysisRequest
{
    [MaxLength(50)]
    public string Language { get; set; } = "C#";

    [Required(ErrorMessage = "Error logs cannot be empty")]
    [MaxLength(50000, ErrorMessage = "Error logs exceed maximum size of 50000 characters")]
    public string ErrorLogs { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source code cannot be empty")]
    [MaxLength(100000, ErrorMessage = "Source code exceeds maximum size of 100000 characters")]
    public string SourceCode { get; set; } = string.Empty;
}