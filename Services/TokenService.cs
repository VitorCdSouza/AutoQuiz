using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoQuizApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace AutoQuizApi.Services;

public class TokenServices : ITokenService
{
    private readonly SymmetricSecurityKey _key;

    public TokenServices(IConfiguration config)
    {
        var secretKey = config["Jwt:Key"] ?? throw new ArgumentException("JWT Key not found in configuration");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        SigningCredentials creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
