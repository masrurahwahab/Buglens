using Buglens.DTOs;
using Buglens.DTOs.Response;

namespace Buglens.Contract.IServices;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> OAuthLoginAsync(string email, string fullName, string provider, string providerId, string profilePictureUrl);
    Task<bool> UserExistsAsync(string email);
    
    Task<ForgotPasswordResponse> ForgotPasswordAsync(string email);
    Task<ForgotPasswordResponse> ResetPasswordAsync(string token, string newPassword);
}