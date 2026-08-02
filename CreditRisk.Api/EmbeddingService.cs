using System.Net.Http.Json;
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
}