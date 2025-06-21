using AutoQuizApi.Data;
using AutoQuizApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoQuizApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuizController : BaseController
{
    private readonly AutoQuizDbContext _context;
    private readonly IAiQuizService _aiQuiz;

    public QuizController(AutoQuizDbContext context, IAiQuizService aiQuiz)
    {
        _context = context;
        _aiQuiz = aiQuiz;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadAndGenerateQuiz(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File not found");
        }

        // ... (código para pegar userId, criar SourceDocument, etc.) ...


        var aiResponse = await _aiQuiz.GenerateQuizFromFileAsync(file);

        return Ok();
    }
}
