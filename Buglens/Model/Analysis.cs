using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buglens.Model;

public class Analysis
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string UserId { get; set;}

    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = "C#";

    [Required]
    [Column(TypeName = "TEXT")]
    public string ErrorLogs { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string SourceCode { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string RootCause { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Explanation { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Fix { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string CorrectedCode { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Success { get; set; } = true;

    [Column(TypeName = "TEXT")]
    public string? ErrorMessage { get; set; }
}