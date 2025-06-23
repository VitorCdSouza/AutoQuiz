using AutoQuizApi.Data;
using AutoQuizApi.Interfaces;
using AutoQuizApi.Models;

namespace AutoQuizApi.Repositories;

public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
{
    private readonly AutoQuizDbContext _context;

    public QuizRepository(AutoQuizDbContext context) : base(context)
    {
        _context = context;
    }
}
