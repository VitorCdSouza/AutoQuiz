using AutoQuizApi.Models;

namespace AutoQuizApi.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
