using AutoQuizApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoQuizApi.Data;

public class AutoQuizDbContext : DbContext
{
    public AutoQuizDbContext(DbContextOptions<AutoQuizDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<SourceDocument> SourceDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
