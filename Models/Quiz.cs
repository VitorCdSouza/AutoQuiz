using System.ComponentModel.DataAnnotations;

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
    public Guid SourceDocumentId { get; set; }
}
