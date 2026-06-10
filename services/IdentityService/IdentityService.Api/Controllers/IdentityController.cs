using IdentityService.External.Contracts.ServiceContracts; 
using IdentityService.Internal.Contracts.DataContracts.Requests; 
using IdentityService.Internal.Contracts.DataContracts.Responses; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api.Controllers; 

[ApiController] 
[Route("api/[controller]")] 
public sealed class IdentityController : ControllerBase 
{ 
    private readonly IIdentityManager _identityManager; 

    public IdentityController(IIdentityManager identityManager) 
    { 
        _identityManager = identityManager; 
    } 

    /// <summary> 
    /// Authenticates a user and returns a token embedded with Role and DOB claims for Angular.
    /// </summary> 
    [HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
{
    // Controller ➔ Manager Only
    var response = await _identityManager.AuthenticateUserAsync(request, cancellationToken);

    if (!response.Success)
    {
        return Unauthorized(new { Message = response.Message });
    }

    // Token creation happens safely at the edge interface layer based on clean manager data
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes("YourSuperSecretBackupKeyThatIsAtLeast32CharsLong!");
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, response.Username),
            new Claim(ClaimTypes.Role, response.Role),
            new Claim("date_of_birth", response.DateOfBirth.ToString("yyyy-MM-dd"))
        }),
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Ok(new { Token = tokenHandler.WriteToken(token) });
}
    /// <summary>
    /// Test endpoint that will FAIL for our admin user (Requires Age > 30)
    /// </summary>
    [HttpPost("test-restricted-age")]
    [Authorize(Policy = "Above30Policy")] // 🔥 Enforces the failing rule
    public IActionResult TestRestrictedAge()
    {
        return Ok(new { Message = "Access Granted: You are verified to be over 30 years old." });
    }

    /// <summary>
    /// Test endpoint that will SUCCEED for our admin user (Requires Admin role or Age > 21)
    /// </summary>
    [HttpPost("test-standard-access")]
    [Authorize(Policy = "Above21Policy")] // 🔥 Enforces the passing rule
    public IActionResult TestStandardAccess()
    {
        return Ok(new { Message = "Access Granted: Multi-tenant administrative token verified successfully!" });
    }
}

// Simple request payload binding model
public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}