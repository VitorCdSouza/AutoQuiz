using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoQuizApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    [Required]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    [Required]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    [NotMapped]
    public string Password { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<Quiz> Quizzes = new List<Quiz>();

    public User()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
