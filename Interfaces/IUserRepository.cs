using AutoQuizApi.Models;

namespace AutoQuizApi.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

