using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoQuizApi.Models;

public class Quiz
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public Guid SourceDocumentId { get; set; }

    [ForeignKey(nameof(SourceDocumentId))]
    public virtual SourceDocument SourceDocument { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public Quiz()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
