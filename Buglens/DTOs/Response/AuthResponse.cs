namespace Buglens.DTOs.Response;

public class AuthResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
}