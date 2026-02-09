using System.Text.Json;
using Buglens.Model;
using Buglens.Models;

public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleTokenResponse> ExchangeGoogleCodeAsync(string code)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            var clientSecret = _configuration["OAuth:Google:ClientSecret"];
            var redirectUri = _configuration["OAuth:Google:RedirectUri"];

            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest)
            );

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Google code for token");
            throw;
        }
    }

    public async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Google API error: {response.StatusCode}, Content: {errorContent}");
                throw new Exception($"Failed to get user info from Google: {response.StatusCode}");
            }

            var userInfoJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Google user info JSON: {userInfoJson}");
        
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson, options);
        
            if (userInfo == null)
            {
                _logger.LogError("Deserialized user info is null");
                throw new Exception("Failed to deserialize Google user info");
            }

            if (string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogError($"Email is null in user info. Full JSON: {userInfoJson}");
                throw new Exception("Google user info does not contain email");
            }

            if (string.IsNullOrEmpty(userInfo.Id))
            {
                _logger.LogError($"ID is null in user info. Full JSON: {userInfoJson}");
                throw new Exception("Google user info does not contain ID");
            }
        
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
            throw;
        }
    }

   public async Task<GitHubTokenResponse> ExchangeGitHubCodeAsync(string code)
{
    try
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];
        var redirectUri = _configuration["OAuth:GitHub:RedirectUri"];

        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(tokenRequest)
        };
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"GitHub token response: {responseContent}");
        
        var tokenResponse = JsonSerializer.Deserialize<GitHubTokenResponse>(responseContent);
        
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            throw new Exception("Failed to get access token from GitHub");
        }
        
        return tokenResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error exchanging GitHub code for token");
        throw;
    }
}

public async Task<GitHubUserInfo> GetGitHubUserInfoAsync(string accessToken)
{
    try
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BugLens");

        var response = await _httpClient.GetAsync("https://api.github.com/user");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"GitHub API error: {response.StatusCode}, Content: {errorContent}");
            throw new Exception($"Failed to get user info from GitHub: {response.StatusCode}");
        }

        var userInfoJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"GitHub user info JSON: {userInfoJson}");
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var userInfo = JsonSerializer.Deserialize<GitHubUserInfo>(userInfoJson, options);
        
        if (userInfo == null)
        {
            _logger.LogError("Deserialized GitHub user info is null");
            throw new Exception("Failed to deserialize GitHub user info");
        }

        
        if (string.IsNullOrEmpty(userInfo.Email))
        {
            _logger.LogWarning("GitHub user has no public email, using login as fallback");
        }
        
        return userInfo;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting GitHub user info");
        throw;
    }
}

}
