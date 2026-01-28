using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Buglens.Contract.IServices;
using Buglens.DTOs;
using Buglens.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IAnalysisService analysisService,
        ILogger<AnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

   
    private string GetUserId()
    {
     
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue("sub");
    }

    [HttpPost]
    [ProducesResponseType(typeof(AnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAnalysis([FromBody] AnalysisRequest request)
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
            var userId = GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized(new ErrorResponse
                {
                    Error = "Unauthorized",
                    Details = "User not authenticated"
                });
            }

            var response = await _analysisService.CreateAnalysisAsync(request, userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create analysis");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error",
                Details = "Failed to process analysis. Please try again later."
            });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalysis(int id)
    {
        try
        {
            var userId = GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized(new ErrorResponse
                {
                    Error = "Unauthorized",
                    Details = "User not authenticated"
                });
            }

            var analysis = await _analysisService.GetAnalysisByIdAsync(id, userId);
            
            if (analysis == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Analysis not found",
                    Details = $"No analysis found with ID {id}"
                });
            }

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analysis");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error",
                Details = "Failed to retrieve analysis."
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AnalysisResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalysisHistory([FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
              
                _logger.LogWarning("User ID not found in claims. Available claims:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogWarning($"  {claim.Type}: {claim.Value}");
                }
                
                return Unauthorized(new ErrorResponse
                {
                    Error = "Unauthorized",
                    Details = "User not authenticated"
                });
            }

            var history = await _analysisService.GetAnalysisHistoryAsync(userId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analysis history");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error",
                Details = "Failed to retrieve analysis history."
            });
        }
    }

    [HttpGet("demo")]
    [ProducesResponseType(typeof(AnalysisRequest), StatusCodes.Status200OK)]
    public IActionResult GetDemoData()
    {
        return Ok(new AnalysisRequest
        {
            Language = "C#",
            ErrorLogs = @"System.NullReferenceException: Object reference not set to an instance of an object.
                   at BugLens.Services.UserService.GetUserById(Int32 id) in C:\Projects\BugLens\Services\UserService.cs:line 45
                   at BugLens.Controllers.UserController.GetUser(Int32 id) in C:\Projects\BugLens\Controllers\UserController.cs:line 23",
            SourceCode = @"public class UserService
            {
                private List<User> _users;

                public User GetUserById(int id)
                {
                    return _users.FirstOrDefault(u => u.Id == id);
                }
            }"
        });
    }
}