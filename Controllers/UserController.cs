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
public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenRepository;
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
            return BadRequest("Este e-mail já está em uso.");
        }

        using var hmac = new HMACSHA512();

        var user = new User
        {
            Email = userDto.Email.ToLower(),
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password))
        };

        await _userRepository.AddAsync(user);
        await _dbcontext.SaveChangesAsync();

        return Ok(new { message = "Usuário registrado com sucesso." });
    }
}
