using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/authorization")]
public sealed class AuthorizationController : ControllerBase
{
    [Authorize(Policy = PermissionCodes.InventoryRead)]
    [HttpGet("inventory-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult InventoryRead()
    {
        return Ok(new
        {
            Message = "Inventory read permission granted."
        });
    }
}
