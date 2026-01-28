using System.Text;
using System.Text.Json;
using Buglens.Contract.IServices;
using Buglens.Model;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private const int MaxRetries = 2;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"]
                  ?? throw new ArgumentNullException("Gemini:ApiKey", "Gemini API key not configured");

       
        _httpClient.Timeout = TimeSpan.FromSeconds(60);

        _logger.LogInformation("GeminiService initialized successfully");
    }

    public async Task<GeminiAnalysisResult> AnalyzeCodeAsync(string language, string errorLogs, string sourceCode)
    {
        var prompt = BuildAnalysisPrompt(language, errorLogs, sourceCode);

        int attempt = 0;
        Exception? lastException = null;

        while (attempt < MaxRetries)
        {
            attempt++;
            try
            {
                _logger.LogInformation("Calling Gemini API (attempt {Attempt}/{MaxRetries})", attempt, MaxRetries);

                var response = await CallGeminiAPIAsync(prompt);

                _logger.LogInformation("Successfully received response from Gemini API");

                return ParseGeminiResponse(response);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "Gemini API call failed (attempt {Attempt}/{MaxRetries})", attempt, MaxRetries);

                if (attempt < MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogInformation("Retrying in {Delay} seconds...", delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
        }

        throw new Exception("Failed to get response from Gemini API after retry", lastException);
    }

    private string BuildAnalysisPrompt(string language, string errorLogs, string sourceCode)
    {
        return
            $@"You are an expert code debugging assistant. Analyze the following code error and provide a detailed analysis.

            **Programming Language:** {language}

            **Error Logs:**
            ```
            {errorLogs}
            ```

            **Source Code:**
            ```{language.ToLower()}
            {sourceCode}
            ```

            Please provide your analysis in the following JSON format (output ONLY valid JSON, no markdown, no explanations):

            {{
              ""rootCause"": ""Brief description of the root cause"",
              ""explanation"": ""Detailed explanation of what's causing the error and why it happens"",
              ""suggestedFix"": ""Step-by-step instructions on how to fix the issue"",
              ""correctedCode"": ""The complete corrected version of the source code""
            }}

            IMPORTANT: Return ONLY the JSON object, nothing else. No markdown formatting, no code blocks, just pure JSON.";
    }

    public async Task<string> ListAvailableModelsAsync()
    {
        try
        {
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}";
            var response = await _httpClient.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Available models: {Models}", content);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list models");
            throw;
        }
    }

    private async Task<string> CallGeminiAPIAsync(string prompt)
    {
        try
        {
            var apiUrl =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 8192, 
                    topP = 0.8,
                    topK = 40
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Gemini API...");

            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Gemini API Response Status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API Error Response: {Response}", responseContent);
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Gemini API failed");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request to Gemini API timed out");
            throw new Exception("Request timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Gemini API");
            throw;
        }
    }

    private GeminiAnalysisResult ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            _logger.LogDebug("Parsing Gemini response");

            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

           
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                throw new Exception("No candidates in Gemini response");
            }

            var firstCandidate = candidates[0];

            if (firstCandidate.TryGetProperty("finishReason", out var finishReason))
            {
                var reason = finishReason.GetString();
                if (reason == "MAX_TOKENS")
                {
                    _logger.LogWarning("Gemini response was truncated due to MAX_TOKENS");
                    throw new Exception("Response truncated - increase maxOutputTokens");
                }
            }

            if (!firstCandidate.TryGetProperty("content", out var content))
            {
                throw new Exception("No content in Gemini response");
            }

            if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
            {
                throw new Exception("No parts in Gemini response");
            }

            var firstPart = parts[0];
            if (!firstPart.TryGetProperty("text", out var textElement))
            {
                throw new Exception("No text in Gemini response");
            }

            var text = textElement.GetString() ?? string.Empty;

            _logger.LogInformation($"Raw Gemini text response: {text}");


            text = text.Trim();
            if (text.StartsWith("```json"))
            {
                text = text.Substring(7);
            }
            else if (text.StartsWith("```"))
            {
                text = text.Substring(3);
            }

            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }

            text = text.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            
            var analysisResult = JsonSerializer.Deserialize<GeminiAnalysisResult>(text, options);

            if (analysisResult == null)
            {
                throw new Exception("Failed to deserialize analysis result");
            }

            _logger.LogInformation("Successfully parsed Gemini response");

            return analysisResult;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response as JSON. Response was: {Response}", jsonResponse);

           
            return new GeminiAnalysisResult
            {
                RootCause = "Response Truncated",
                Explanation =
                    "The AI response was too long and got cut off. Please try with shorter code or error messages.",
                SuggestedFix = "Reduce the length of your error logs or source code, then try again.",
                CorrectedCode = ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            throw;
        }
    }
}
