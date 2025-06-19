using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoQuizApi.Models;

public class Answer
{
    [Key]
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; } = false;

    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; } = null!;
}
