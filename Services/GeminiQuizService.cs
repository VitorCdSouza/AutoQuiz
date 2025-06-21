using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoQuizApi.Models;

namespace AutoQuizApi.Services;

public class GeminiQuizService : IAiQuizService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;


    public GeminiQuizService(IConfiguration config)
    {
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini API Key not found");
        _httpClient = new HttpClient();
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        using StreamContent fileContent = new StreamContent(file.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/files?key={_apiKey}");
        request.Content = fileContent;
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();

        JsonElement fileObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
        return fileObject.GetProperty("file").GetString()!;
    }

    public async Task<AiQuizDto> GenerateQuizFromFileAsync(IFormFile file)
    {
        string fileUri = await UploadFileAsync(file);
        await Task.Delay(5000);

        string prompt = "Baseado no documento fornecido, crie um quiz. Responda APENAS com um objeto JSON válido no seguinte formato: { \"quizName\": \"Nome do Quiz\", \"questions\": [ { \"text\": \"Enunciado da pergunta\", \"answers\": [ { \"text\": \"Texto da resposta\", \"isCorrect\": true/false } ] } ] }";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new object[] {
                    new { text = prompt },
                    new { file_data = new { mime_type = file.ContentType, file_uri = fileUri } }
                }}
            }
        };

        string jsonRequest = JsonSerializer.Serialize(requestBody);
        StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}", content);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();

        return new AiQuizDto { QuizTitle = "Quiz Gerado pelo Gemini", Questions = new() };
    }
}