using AutoQuizApi.Models;

namespace AutoQuizApi.Services;

public class MockAiQuizService : IAiQuizService
{
    public Task<AiQuizDto> GenerateQuizFromFileAsync(IFormFile fileContent)
    {
        Task.Delay(2000).Wait();

        var mockResponse = new AiQuizDto
        {
            QuizTitle = "Quiz Simulado a partir de PDF",
            Questions = new List<AiQuestionDto>
            {
                new AiQuestionDto
                {
                    Text = "Qual a capital do Brasil?",
                    Answers = new List<AiAnswerDto>
                    {
                        new AiAnswerDto { Text = "São Paulo", IsCorrect = false },
                        new AiAnswerDto { Text = "Rio de Janeiro", IsCorrect = false },
                        new AiAnswerDto { Text = "Brasília", IsCorrect = true },
                        new AiAnswerDto { Text = "Belo Horizonte", IsCorrect = false }
                    }
                },
                new AiQuestionDto
                {
                    Text = "Qual o resultado de 2 + 2?",
                    Answers = new List<AiAnswerDto>
                    {
                        new AiAnswerDto { Text = "4", IsCorrect = true },
                        new AiAnswerDto { Text = "5", IsCorrect = false }
                    }
                }
            }
        };

        return Task.FromResult(mockResponse);
    }
}