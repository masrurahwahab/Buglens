using System.Net;
using System.Security.Claims;
using Buglens.Contract.IRepository;
using Buglens.Contract.IServices;
using BugLens.Data;
using Buglens.Repository;
using Buglens.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Buglens.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

// Diagnostics
Console.WriteLine("=== Configuration Diagnostics ===");
Console.WriteLine($"Jwt:Key exists: {!string.IsNullOrEmpty(builder.Configuration["Jwt:Key"])}");
Console.WriteLine($"Jwt:Issuer: {builder.Configuration["Jwt:Issuer"]}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine("=== End Diagnostics ===");

// Configure Data Protection for OAuth state cookies
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/asp-dataprotection-keys"))
    .SetApplicationName("BugLens");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BugLensContext>(options =>
    options.UseNpgsql(connectionString));

// JWT (optional, still needed for API authentication)
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub"; // Default OAuth challenge
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ HTTPS on Render
    options.Cookie.HttpOnly = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
.AddGoogle("Google", google =>
{
    google.ClientId = builder.Configuration["OAuth:Google:ClientId"] ?? throw new ArgumentNullException("Google ClientId missing");
    google.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"] ?? throw new ArgumentNullException("Google ClientSecret missing");
    google.CallbackPath = "/api/OAuth/google/callback";
})
.AddGitHub("GitHub", github =>
{
    github.ClientId = builder.Configuration["OAuth:GitHub:ClientId"];
    github.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"];
    github.CallbackPath = "/api/OAuth/github/callback";
    github.Scope.Add("read:user");
    github.Scope.Add("user:email");

    // HTTPS cookie settings for Render
    github.CorrelationCookie.SameSite = SameSiteMode.Lax;
    github.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ HTTPS on Render
});

// Repositories and Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddHttpClient<IOAuthService, OAuthService>();
builder.Services.AddScoped<Buglens.UnitOfWork.IUnitOfWork, Buglens.UnitOfWork.UnitOfWork>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        };
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("AllowAll");

// Static files
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("welcome.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
