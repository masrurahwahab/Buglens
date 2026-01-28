using System.ComponentModel.DataAnnotations;

namespace Buglens.Model;


public class User
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }
        
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [MaxLength(255)]
    public string? PasswordHash { get; set; } 
        
 
    [MaxLength(50)]
    public string? Provider { get; set; } // "Google", "GitHub", "Email"
        
    [MaxLength(255)]
    public string? ProviderId { get; set; }
        
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
        
    [MaxLength(50)]
    public string Role { get; set; } = "Developer";
        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    public DateTime? LastLoginAt { get; set; }
        
    public bool IsActive { get; set; } = true;
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
}