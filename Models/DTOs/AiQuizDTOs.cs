namespace AutoQuizApi.Models;

public class AiAnswerDto
{
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}

public class AiQuestionDto
{
    public required string Text { get; set; }
    public List<AiAnswerDto> Answers { get; set; } = new List<AiAnswerDto>();
}

public class AiQuizDto
{
    public required string QuizTitle { get; set; }
    public List<AiQuestionDto> Questions { get; set; } = new List<AiQuestionDto>();
}