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
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// IMPORTANT: Explicitly add environment variables
builder.Configuration.AddEnvironmentVariables();

// Diagnostic logging to verify config is loaded
Console.WriteLine("=== Configuration Diagnostics ===");
Console.WriteLine($"Jwt:Key exists: {!string.IsNullOrEmpty(builder.Configuration["Jwt:Key"])}");
Console.WriteLine($"Jwt:Key length: {builder.Configuration["Jwt:Key"]?.Length ?? 0}");
Console.WriteLine($"Jwt:Issuer: {builder.Configuration["Jwt:Issuer"]}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine("=== End Diagnostics ===");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BugLensContext>(options =>
    options.UseNpgsql(connectionString));



// Get JWT settings with null checks
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key configuration is missing! Please check your environment variables.");
}

if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("Jwt:Issuer configuration is missing!");
}

if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("Jwt:Audience configuration is missing!");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Console.WriteLine("Token has expired");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($" OnChallenge: Error={context.Error}, Description={context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"Authorization header: {authHeader?.Substring(0, Math.Min(50, authHeader?.Length ?? 0))}...");
                return Task.CompletedTask;
            }
        };
    
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
    });

        
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
        var handler = new HttpClientHandler
        {
           
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            
          
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            
           
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        };
        
        return handler;
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

         var googleClientId = builder.Configuration["GOOGLE:CLIENT:ID"];
var googleClientSecret = builder.Configuration["GOOGLE:CLIENT:SECRET"];
var githubClientId = builder.Configuration["GITHUB:CLIENT:ID"];
var githubClientSecret = builder.Configuration["GITHUB:CLIENT:SECRET"];

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/OAuth/google/callback";
    })
    .AddGitHub(options =>
    {
        options.ClientId = githubClientId;
        options.ClientSecret = githubClientSecret;
        options.CallbackPath = "/api/OAuth/github/callback";
    });



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("welcome.html");
app.UseDefaultFiles(defaultFilesOptions);

app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
