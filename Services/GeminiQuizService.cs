using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    private async Task WaitForFileToBeActiveAsync(string fileId, int timeoutSeconds = 60)
    {
        string getFileUrl = $"https://generativelanguage.googleapis.com/v1beta/{fileId}?key={_apiKey}";
        DateTime startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(timeoutSeconds)) // timeout validations with default of 60 s
        {
            HttpResponseMessage response = await _httpClient.GetAsync(getFileUrl);

            // wrong response code received
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error at File: {fileId}. Status: {response.StatusCode}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);

            if (!doc.RootElement.TryGetProperty("state", out var stateElement))
            {
                throw new InvalidOperationException($"Api response does not contain 'state' {jsonResponse}");
            }

            var state = stateElement.GetString();
            Console.WriteLine($"File '{fileId}' is {state}");

            if (state == "ACTIVE")
            {
                return;
            }

            if (state == "FAILED")
            {
                throw new InvalidOperationException($"File: {fileId} failed at Google's API.");
            }

            await Task.Delay(3000);
        }

        throw new TimeoutException($"File: {fileId} took more than {timeoutSeconds} seconds.");
    }

    public async Task<(string fileUri, string fileId)> UploadFileAsync(IFormFile file)
    {
        string uploadUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/files?key={_apiKey}";

        // using data stream as better practice
        // instead of upload full file to the url, it sends constant minor packages

        // creating a stream using 'file' as base
        using StreamContent streamContent = new StreamContent(file.OpenReadStream());

        // looking at file type to use as header
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        // creating and populating the request
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Headers.Add("X-Goog-Upload-File-Name", file.FileName);
        request.Content = streamContent;

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // validating response
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Upload fail. Status: {response.StatusCode}. Response: {errorContent}");
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();

        // converting and looking at reponse as json
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement fileObject = doc.RootElement.GetProperty("file");
        string? fileUri = fileObject.GetProperty("uri").GetString();
        string? fileId = fileObject.GetProperty("name").GetString();

        if (string.IsNullOrEmpty(fileUri) || string.IsNullOrEmpty(fileId))
        {
            throw new InvalidOperationException("Not able to process file uri or file id");
        }

        return (fileUri, fileId);
    }

    public async Task<AiQuizDto> GenerateQuizFromFileAsync(IFormFile file)
    {
        Console.WriteLine("Uploading file...\n");

        (string fileUri, string fileId) = await UploadFileAsync(file);
        Console.WriteLine($"Upload done. ID: {fileId}, URI: {fileUri}");

        Console.WriteLine("Awaiting API processing");
        await WaitForFileToBeActiveAsync(fileId);
        Console.WriteLine("File is 'ACTIVE'");

        // sending and receiving quiz in portuguese, do not know if I will do more languages
        string prompt = "Baseado no documento fornecido, crie um quiz. Responda APENAS com um objeto JSON válido no seguinte formato: { \"quizTitle\": \"Nome do Quiz\", \"questions\": [ { \"text\": \"Enunciado da pergunta\", \"answers\": [ { \"text\": \"Texto da resposta\", \"isCorrect\": true/false } ] } ] }";

        object requestBody = new
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

        string model = "gemini-2.5-flash";
        HttpResponseMessage response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}", content);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();

        using JsonDocument apiDoc = JsonDocument.Parse(jsonResponse);
        string rawTextFromGemini = apiDoc.RootElement
                                         .GetProperty("candidates")[0]
                                         .GetProperty("content")
                                         .GetProperty("parts")[0]
                                         .GetProperty("text")
                                         .GetString() ?? string.Empty;

        Match match = Regex.Match(rawTextFromGemini, @"\{.*\}", RegexOptions.Singleline);

        if (!match.Success)
        {
            Console.WriteLine($"Não foi possível extrair um JSON válido da resposta do Gemini. Resposta recebida: {rawTextFromGemini}");
            throw new InvalidOperationException("A resposta da IA não continha um objeto JSON válido.");
        }

        string cleanedJson = match.Value;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        AiQuizDto? quiz = JsonSerializer.Deserialize<AiQuizDto>(cleanedJson, options);

        if (quiz == null)
        {
            throw new JsonException("Error at json deserialize");
        }

        return quiz;
    }
}