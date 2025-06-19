using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoQuizApi.Controllers;

[Authorize]
public class BaseController : ControllerBase
{
    protected int GetLoggedInUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
        }
        return 0;
    }
}
