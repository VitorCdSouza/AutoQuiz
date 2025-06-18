using AutoQuizApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoQuizApi.Controllers;

[Authorize]
public class BaseController : ControllerBase
{
    protected int GetLoggedUserId()
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        int userId = 0;
        if (!Int32.TryParse(Encryption.GetIdUserByToken(token), out userId))
        {
            return 0;
        }
        return userId;
    }
}
