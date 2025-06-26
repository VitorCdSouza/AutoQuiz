using System.Security.Cryptography;
using System.Text;
using AutoQuizApi.Data;
using AutoQuizApi.Interfaces;
using AutoQuizApi.Models;
using AutoQuizApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoQuizApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenRepository; // CreateToken method
    private readonly AutoQuizDbContext _dbcontext;

    public UserController(IUserRepository userRepository, ITokenService tokenService, AutoQuizDbContext dbcontext)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenService;
        _dbcontext = dbcontext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto userDto)
    {
        if (await _userRepository.GetByEmailAsync(userDto.Email) != null)
        {
            return BadRequest("Email already in use");
        }

        using HMACSHA512 hmac = new();
        User user = new()
        {
            Email = userDto.Email.ToLower(),
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password))
        };

        await _userRepository.AddAsync(user);
        await _dbcontext.SaveChangesAsync();

        return Ok(new { message = "User registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthUserDto userDto)
    {
        User? user = await _userRepository.GetByEmailAsync(userDto.Email);
        if (user == null)
        {
            return Unauthorized("Email or password wrong");
        }

        using HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt);
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Email or password wrong");
            }
        }

        string token = _tokenRepository.CreateToken(user);

        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true,
        };

        Response.Cookies.Append("authToken", token, cookieOptions);

        return Ok(new
        {
            message = "Logged in",
            user = new { user.Email, user.Id },
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1),
            Secure = true,
        };

        Response.Cookies.Append("authToken", "", cookieOptions);

        return Ok(new
        {
            message = "Logged off",
        });
    }
}
