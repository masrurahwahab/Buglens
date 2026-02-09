using System.Net;
using System.Text;
using BugLens.Data;
using Buglens.Contract.IRepository;
using Buglens.Contract.IServices;
using Buglens.Repository;
using Buglens.Service;
using Buglens.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();


// =======================
// DATA PROTECTION (Fix OAuth State)
// =======================
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/asp-dataprotection-keys"))
    .SetApplicationName("BugLens");


// =======================
// SERVICES
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


// =======================
// DATABASE
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BugLensContext>(options =>
    options.UseNpgsql(connectionString));


// =======================
// JWT CONFIG
// =======================
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;


// =======================
// AUTHENTICATION
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})


// =======================
// GOOGLE OAUTH
// =======================
.AddGoogle("Google", google =>
{
    google.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
    google.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
    google.CallbackPath = "/api/OAuth/google/callback";

    google.CorrelationCookie.SameSite = SameSiteMode.Lax;
    google.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
})


// =======================
// GITHUB OAUTH
// =======================
.AddGitHub("GitHub", github =>
{
    github.ClientId = builder.Configuration["OAuth:GitHub:ClientId"]!;
    github.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"]!;
    github.CallbackPath = "/api/OAuth/github/callback";

    github.Scope.Add("read:user");
    github.Scope.Add("user:email");

    github.CorrelationCookie.SameSite = SameSiteMode.Lax;
    github.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
});


// =======================
// REPOSITORIES & SERVICES
// =======================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddHttpClient<IOAuthService, OAuthService>();

builder.Services.AddScoped<Buglens.UnitOfWork.IUnitOfWork,
    Buglens.UnitOfWork.UnitOfWork>();

builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHttpClient<IGeminiService, GeminiService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            AutomaticDecompression =
                DecompressionMethods.GZip | DecompressionMethods.Deflate
        });


// =======================
// BUILD APP
// =======================
var app = builder.Build();


// =======================
// RENDER PORT BINDING
// =======================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");


// =======================
// HTTPS REDIRECTION
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// =======================
// SWAGGER
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// =======================
// MIDDLEWARE
// =======================
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();


// Static Files
var defaultFiles = new DefaultFilesOptions();
defaultFiles.DefaultFileNames.Clear();
defaultFiles.DefaultFileNames.Add("welcome.html");

app.UseDefaultFiles(defaultFiles);
app.UseStaticFiles();


// Controllers
app.MapControllers();

app.Run();
