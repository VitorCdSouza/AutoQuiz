using System.ComponentModel.DataAnnotations;

namespace AutoQuizApi.Models;

public class SourceDocument
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(150)]
    public required string OriginalFileName { get; set; } = string.Empty;

    [MaxLength(150)]
    public required string StoredFileName { get; set; } = string.Empty;

    public required string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public SourceDocument()
    {
        UploadedAt = DateTime.UtcNow;
    }
}
