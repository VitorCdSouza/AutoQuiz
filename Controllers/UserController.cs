using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoQuizApi.Controllers;
using AutoQuizApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : BaseController
{
    private readonly ExpenseTrackerContext _context;
    private readonly IConfiguration _configuration;

    public UserController(ExpenseTrackerContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private string CreateToken(User usr)
    {
        List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Actor, usr.Id.ToString()),
                new Claim(ClaimTypes.Email, usr.Email),
            };

        string? keyValue = _configuration.GetSection("TokenConfiguration:Key").Value;
        if (string.IsNullOrEmpty(keyValue))
        {
            throw new InvalidOperationException("Token key is not configured.");
        }

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMonths(1),
            SigningCredentials = creds
        };
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    [HttpGet]
    public async Task<ActionResult<User>> GetUserById()
    {
        int userId = GetUserIdToken();

        User? user = await _context.Users.Include(a => a.Account).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return BadRequest("User does not exist");
        }

        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<ActionResult<User>> RegisterUser(CreateUserDto dto)
    {
        if (dto.Email == null || dto.Password == null || dto.Name == null)
        {
            return BadRequest("Empty fields");
        }

        User user = new User
        {
            Email = dto.Email,
            Password = dto.Password
        };

        Account acc = new Account
        {
            Name = dto.Name,
            User = user,
        };

        Encryption.CreatePasswordHash(user.Password, out byte[] hash, out byte[] salt);
        user.Password = string.Empty;
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        user.Account = acc;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult<User>> LoginUser(AuthUserDto dto)
    {
        if (dto.Email == null || dto.Password == null)
        {
            return BadRequest("Empty fields");
        }

        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            return BadRequest("Invalid credentials");
        }
        else if (!Encryption.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest("Invalid credentials");
        }

        string token = CreateToken(user);

        return Ok(token);
    }
}
