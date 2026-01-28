

using Buglens.Model;

using Buglens.Models;

public interface IOAuthService
{
    Task<GoogleTokenResponse> ExchangeGoogleCodeAsync(string code);
    Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken);
    Task<GitHubTokenResponse> ExchangeGitHubCodeAsync(string code);
    Task<GitHubUserInfo> GetGitHubUserInfoAsync(string accessToken);
}