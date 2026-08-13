using System.Net.Http.Json;
using CreditRisk.Core;
using Microsoft.Extensions.Logging;
using Pgvector;

public class EmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(IHttpClientFactory factory, IConfiguration config, ILogger<EmbeddingService> logger)
    {
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri(config["Ollama:Url"] ?? "http://localhost:11434");
        _logger = logger;
    }

    public async Task<Vector> GetEmbeddingAsync(string text)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/embeddings", new
            {
                model = "nomic-embed-text",
                prompt = text
            });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
            return new Vector(result!.Embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get embedding from Ollama. Is it running on the configured URL?");
            throw;
        }
    }

    private class EmbeddingResponse
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public static string BuildContent(LoanApplication app)
    {
        return
            $"Application {app.Id}: {app.FullName}, age {app.Age}. " +
            $"Loan of {app.CreditAmount:C0} over {app.DurationMonths} months for {FeatureLabels.Purpose(app.Purpose)}. " +
            $"Checking: {FeatureLabels.CheckingStatus(app.CheckingStatus)}. " +
            $"Credit history: {FeatureLabels.CreditHistory(app.CreditHistory)}. " +
            $"Employment: {FeatureLabels.Employment(app.Employment)}. " +
            $"Decision: {app.Decision}.";
    }
}