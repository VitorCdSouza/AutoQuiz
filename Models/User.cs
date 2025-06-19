using System.ComponentModel.DataAnnotations;

namespace AutoQuizApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public required byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public required byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public User()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
