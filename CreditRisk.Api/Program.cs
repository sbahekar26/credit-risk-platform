
using CreditRisk.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pgvector.EntityFrameworkCore;
using NpgsqlTypes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<CreditRiskPredictor>();
builder.Services.AddScoped<ApplicationStatsService>();

builder.Services.AddDbContext<CreditRiskDbContext>(options =>
    options.UseNpgsql("Host=db;Port=5432;Database=creditrisk;Username=postgres;Password=devpassword",
    o => o.UseVector()));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CreditRiskDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapPost("/api/applications", async (LoanApplication application, CreditRiskDbContext db, CreditRiskPredictor predictor, EmbeddingService embedder) =>
{
    // server-side validation
    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(application);
    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(application, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(r => r.ErrorMessage));
    }
    
    application.SubmittedOn = DateTime.UtcNow;
    application.Decision = predictor.Evaluate(application);   // ← was: RiskDecision.Review

    db.LoanApplications.Add(application);
    await db.SaveChangesAsync();

    // auto-embed the new application for RAG
    string content = EmbeddingService.BuildContent(application);

    var vector = await embedder.GetEmbeddingAsync(content);

    db.ApplicationEmbeddings.Add(new ApplicationEmbedding
    {
        LoanApplicationId = application.Id,
        Content = content,
        Embedding = vector
    });
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        applicationId = application.Id,
        applicant = application.FullName,
        decision = application.Decision.ToString()
    });
});

// list all
app.MapGet("/api/applications", async (CreditRiskDbContext db) =>
    await db.LoanApplications.ToListAsync());

// get one by id
app.MapGet("/api/applications/{id}", async (int id, CreditRiskDbContext db) =>
    await db.LoanApplications.FindAsync(id) is LoanApplication app
        ? Results.Ok(app)
        : Results.NotFound());

app.MapGet("/api/ask-test", async (string question) =>
{
    var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
    builder.AddOllamaChatCompletion("llama3.2", new Uri("http://host.docker.internal:11434"));
    var kernel = builder.Build();

    var result = await kernel.InvokePromptAsync(question);
    return Results.Ok(result.ToString());
});

app.MapPost("/api/index-applications", async (CreditRiskDbContext db, EmbeddingService embedder) =>
{
    var applications = await db.LoanApplications.ToListAsync();
    int indexed = 0;

    foreach (var app in applications)
    {
        // build a readable text description of the application
        string content = EmbeddingService.BuildContent(app);

        var vector = await embedder.GetEmbeddingAsync(content);

        db.ApplicationEmbeddings.Add(new ApplicationEmbedding
        {
            LoanApplicationId = app.Id,
            Content = content,
            Embedding = vector
        });
        indexed++;
    }

    await db.SaveChangesAsync();
    return Results.Ok($"Indexed {indexed} applications.");
});

app.MapGet("/api/ask", async (string question, CreditRiskDbContext db, EmbeddingService embedder, ApplicationStatsService stats) =>
{
    // 1. try to answer with a structured query first
    string? statsAnswer = await stats.TryAnswer(question);
    if (statsAnswer != null)
    {
        return Results.Ok(new { question, answer = statsAnswer, source = "database" });
    }

    // 2. fall back to vector RAG for semantic questions
    var questionVector = await embedder.GetEmbeddingAsync(question);

    var relevant = await db.ApplicationEmbeddings
        .OrderBy(e => e.Embedding.CosineDistance(questionVector))
        .Take(3)
        .Select(e => e.Content)
        .ToListAsync();

    string context = string.Join("\n", relevant);
    string prompt =
        $"You are a credit risk assistant. Answer the question using ONLY the application data below.\n\n" +
        $"Application data:\n{context}\n\n" +
        $"Question: {question}\n\nAnswer:";

    var kernelBuilder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
    kernelBuilder.AddOllamaChatCompletion("llama3.2", new Uri("http://host.docker.internal:11434"));
    var kernel = kernelBuilder.Build();
    var result = await kernel.InvokePromptAsync(prompt);

    return Results.Ok(new { question, answer = result.ToString(), source = "rag" });
});

app.Run();

public partial class Program { }