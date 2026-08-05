using System.Net.Http.Json;
using CreditRisk.Core;
using Pgvector;

public class EmbeddingService
{
    private readonly HttpClient _http;

    public EmbeddingService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri("http://localhost:11434");
    }

    public async Task<Vector> GetEmbeddingAsync(string text)
    {
        var response = await _http.PostAsJsonAsync("/api/embeddings", new
        {
            model = "nomic-embed-text",
            prompt = text
        });

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        return new Vector(result!.Embedding);
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