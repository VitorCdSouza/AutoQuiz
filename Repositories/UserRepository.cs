using AutoQuizApi.Data;
using AutoQuizApi.Interfaces;
using AutoQuizApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoQuizApi.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AutoQuizDbContext _context;

    public UserRepository(AutoQuizDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
}
