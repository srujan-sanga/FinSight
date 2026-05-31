using IdentityService.External.Contracts.ServiceContracts;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using Microsoft.AspNetCore.Mvc;

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
    /// Retrieves a user by their unique identifier.
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<GetUserResponse>> GetUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var request = new GetUserRequest
        {
            UserId = userId
        };

        var response = await _identityManager.GetUserAsync(
            request,
            cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}