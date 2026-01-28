 using System.Security.Cryptography;
 using System.Text;
 using Buglens.Contract.IServices;
 using BugLens.Data;
 using Buglens.DTOs;
 using Buglens.DTOs.Response;
 using Buglens.Model;
 using Microsoft.EntityFrameworkCore;

 public class AuthService : IAuthService
    {
        private readonly BugLensContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(BugLensContext context, ITokenService tokenService ,IEmailService emailService,ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await UserExistsAsync(request.Email))
            {
                throw new Exception("User with this email already exists");
            }

            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Provider = "Email",
                Role = "Developer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new Exception("Your account has been deactivated");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<AuthResponse> OAuthLoginAsync(string email, string fullName, string provider, string providerId, string profilePictureUrl)
{
  
    if (string.IsNullOrWhiteSpace(email))
    {
        _logger.LogError("Email is null or empty in OAuthLoginAsync");
        throw new ArgumentException("Email cannot be null or empty", nameof(email));
    }

    if (string.IsNullOrWhiteSpace(providerId))
    {
        _logger.LogError("ProviderId is null or empty in OAuthLoginAsync");
        throw new ArgumentException("Provider ID cannot be null or empty", nameof(providerId));
    }

    _logger.LogInformation($"OAuthLoginAsync called with email: {email}, provider: {provider}, providerId: {providerId}");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    if (user == null)
    {
      
        user = new User
        {
            FullName = fullName ?? "User",
            Email = email.ToLower(),
            Provider = provider,
            ProviderId = providerId,
            ProfilePictureUrl = profilePictureUrl,
            PasswordHash = "[OAUTH_NO_PASSWORD]", 
            Role = "Developer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        _logger.LogInformation($"Created new user from OAuth: {email}");
    }
    else
    {
      
        user.LastLoginAt = DateTime.UtcNow;
        if (string.IsNullOrEmpty(user.ProfilePictureUrl) && !string.IsNullOrEmpty(profilePictureUrl))
        {
            user.ProfilePictureUrl = profilePictureUrl;
        }
        _logger.LogInformation($"Updated existing user: {email}");
    }

    await _context.SaveChangesAsync();

    var token = _tokenService.GenerateToken(user);

    return new AuthResponse
    {
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        Token = token,
        CreatedAt = user.CreatedAt
    };
}
     
        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user exists: {email}");
                throw;
            }
        }
        
      
      

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());

            if (user == null)
            {
               
                return new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "If an account exists with this email, a password reset link has been sent."
                };
            }

            
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); 

            await _context.SaveChangesAsync();

            
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FullName);

            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "If an account exists with this email, a password reset link has been sent."
            };
        }

        public async Task<ForgotPasswordResponse> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (user == null || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired reset token");
            }

            
            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "Password has been reset successfully"
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;
            var hashedInput = HashPassword(password);
            return hashedInput == passwordHash;
        }
    }