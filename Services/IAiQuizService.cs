using AutoQuizApi.Models;

namespace AutoQuizApi.Services;

public interface IAiQuizService
{
    Task<AiQuizDto> GenerateQuizFromFileAsync(IFormFile file);
}