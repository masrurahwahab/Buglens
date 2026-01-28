using Buglens.Contract.IServices;

using Buglens.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Buglens.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Buglens.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid input",
                    Details = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var response = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return BadRequest(new ErrorResponse
                {
                    Error = "Registration failed",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid input",
                    Details = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return Unauthorized(new ErrorResponse
                {
                    Error = "Login failed",
                    Details = ex.Message
                });
            }
        }

      
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
            };
            return Challenge(properties, "Google");
        }

       
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync("Google");

                if (!authenticateResult.Succeeded)
                    return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Google authentication failed");

                var claims = authenticateResult.Principal.Claims;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                var response = await _authService.OAuthLoginAsync(email, name, "Google", providerId, picture);

             
                return Redirect($"{_configuration["Frontend:Url"]}/auth-callback.html?token={response.Token}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google callback failed");
                return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Authentication failed");
            }
        }

        
        [HttpGet("github-login")]
        public IActionResult GitHubLogin([FromQuery] string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GitHubCallback), new { returnUrl })
            };
            return Challenge(properties, "GitHub");
        }
        
     

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid input",
                    Details = "Email is required"
                });
            }

            try
            {
                var response = await _authService.ForgotPasswordAsync(request.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed");
                return BadRequest(new ErrorResponse
                {
                    Error = "Request failed",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid input",
                    Details = "Token and new password are required"
                });
            }

            try
            {
                var response = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed");
                return BadRequest(new ErrorResponse
                {
                    Error = "Reset failed",
                    Details = ex.Message
                });
            }
        }
    
        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback(string returnUrl = "/")
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync("GitHub");

                if (!authenticateResult.Succeeded)
                    return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=GitHub authentication failed");

                var claims = authenticateResult.Principal.Claims;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? email;
                var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var picture = claims.FirstOrDefault(c => c.Type == "urn:github:avatar")?.Value;

                var response = await _authService.OAuthLoginAsync(email, name, "GitHub", providerId, picture);

                return Redirect($"{_configuration["Frontend:Url"]}/auth-callback.html?token={response.Token}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GitHub callback failed");
                return Redirect($"{_configuration["Frontend:Url"]}/login.html?error=Authentication failed");
            }
        }

        [HttpGet("check-email")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var exists = await _authService.UserExistsAsync(email);
            return Ok(new { exists });
        }


    }
}