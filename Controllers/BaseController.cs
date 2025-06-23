using System.Security.Claims;
using System.Threading.Tasks;
using AutoQuizApi.Interfaces;
using AutoQuizApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoQuizApi.Controllers;

[Authorize] // controllers that inherit BaseController will have Authorize annotation
public class BaseController : ControllerBase
{
    protected Guid GetLoggedInUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
        }
        return Guid.Empty;
    }
}
