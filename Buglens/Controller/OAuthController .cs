using Buglens.Contract.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Buglens.API.Controllers;
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOAuthService _oauthService;
        private readonly ILogger<OAuthController> _logger;
        private readonly IConfiguration _configuration;

        public OAuthController(
            IAuthService authService,
            IOAuthService oauthService,
            ILogger<OAuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _oauthService = oauthService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get Google OAuth URL
        /// </summary>
        [HttpGet("google/url")]
        public IActionResult GetGoogleAuthUrl()
        {
            try
            {
                var redirectUri = _configuration["OAuth:Google:RedirectUri"];
                var clientId = _configuration["OAuth:Google:ClientId"];
                
                var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={clientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                    $"response_type=code&" +
                    $"scope=openid%20profile%20email&" +
                    $"access_type=offline&" +
                    $"prompt=consent";

                return Ok(new { url = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google auth URL");
                return StatusCode(500, new { error = "Failed to generate Google auth URL" });
            }
        }

  [HttpGet("google/callback")]
public async Task<IActionResult> GoogleCallback([FromQuery] string code)
{
    try
    {
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("Google callback received without code");
            return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Google login failed - no code");
        }

        _logger.LogInformation("Processing Google OAuth callback with code");

        // Exchange code for tokens
        var tokenResponse = await _oauthService.ExchangeGoogleCodeAsync(code);
        _logger.LogInformation("Successfully exchanged code for token");
        
        // Get user info from Google
        var userInfo = await _oauthService.GetGoogleUserInfoAsync(tokenResponse.access_token);
        _logger.LogInformation($"Retrieved user info - Email: {userInfo?.Email}, Name: {userInfo?.Name}, Id: {userInfo?.Id}");

        // Validate user info
        if (userInfo == null)
        {
            _logger.LogError("User info is null after Google API call");
            return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Failed to get user info from Google");
        }

        if (string.IsNullOrEmpty(userInfo.Email))
        {
            _logger.LogError("User email is null or empty");
            return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Google account has no email");
        }

        if (string.IsNullOrEmpty(userInfo.Id))
        {
            _logger.LogError("User ID is null or empty");
            return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Google account has no ID");
        }

        // Login or register user
        var authResponse = await _authService.OAuthLoginAsync(
            email: userInfo.Email,
            fullName: userInfo.Name ?? "Google User",
            provider: "Google",
            providerId: userInfo.Id,
            profilePictureUrl: userInfo.Picture
        );

        _logger.LogInformation($"Successfully authenticated user: {authResponse.Email}");

        // Redirect to frontend with token
        var redirectUrl = $"{_configuration["Frontend:Url"]}/login.html?" +
            $"token={authResponse.Token}&" +
            $"email={Uri.EscapeDataString(authResponse.Email)}&" +
            $"fullName={Uri.EscapeDataString(authResponse.FullName ?? "User")}&" +
            $"userId={authResponse.UserId}";

        return Redirect(redirectUrl);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during Google OAuth callback: {Message}", ex.Message);
        return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=" + Uri.EscapeDataString($"Google login failed: {ex.Message}"));
    }
}
[HttpGet("github/callback")]
public async Task<IActionResult> GitHubCallback([FromQuery] string code)
{
    try
    {
        if (string.IsNullOrEmpty(code))
        {
            return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=GitHub login failed");
        }

        // Exchange code for access token
        var tokenResponse = await _oauthService.ExchangeGitHubCodeAsync(code);
        
        // Get user info from GitHub
        var userInfo = await _oauthService.GetGitHubUserInfoAsync(tokenResponse.access_token);

        // Login or register user
        var authResponse = await _authService.OAuthLoginAsync(
            email: userInfo.Email ?? $"{userInfo.Login}@github.local",
            fullName: userInfo.Name ?? userInfo.Login,
            provider: "GitHub",
            providerId: userInfo.Id.ToString(),
            profilePictureUrl: userInfo.AvatarUrl
        );

        // Redirect to frontend with token
        var redirectUrl = $"{_configuration["Frontend:Url"]}/login.html?" +
            $"token={authResponse.Token}&" +
            $"email={Uri.EscapeDataString(authResponse.Email)}&" +
            $"fullName={Uri.EscapeDataString(authResponse.FullName)}&" +
            $"userId={authResponse.UserId}";

        return Redirect(redirectUrl);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during GitHub OAuth callback");
        return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=GitHub login failed");
    }
}
        /// <summary>
        /// Get GitHub OAuth URL
        /// </summary>
        [HttpGet("github/url")]
        public IActionResult GetGitHubAuthUrl()
        {
            try
            {
                var redirectUri = _configuration["OAuth:GitHub:RedirectUri"];
                var clientId = _configuration["OAuth:GitHub:ClientId"];
                
                var authUrl = $"https://github.com/login/oauth/authorize?" +
                    $"client_id={clientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                    $"scope=read:user%20user:email";

                return Ok(new { url = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating GitHub auth URL");
                return StatusCode(500, new { error = "Failed to generate GitHub auth URL" });
            }
        }

  
    }  