using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoQuizApi.Models;

public class Question
{
    [Key]
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
