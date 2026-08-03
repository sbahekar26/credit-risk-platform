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
builder.Services.AddDbContext<CreditRiskDbContext>(options => 
options.UseNpgsql("Host=localhost;Port=5432;Database=creditrisk;Username=postgres;Password=devpassword"));

builder.Services.AddDbContext<CreditRiskDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=creditrisk;Username=postgres;Password=devpassword",
        o => o.UseVector()));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();
app.UseCors();

app.UseHttpsRedirection();

app.MapPost("/api/applications", async (LoanApplication application, CreditRiskDbContext db, CreditRiskPredictor predictor, EmbeddingService embedder) =>
{
    application.SubmittedOn = DateTime.UtcNow;
    application.Decision = predictor.Evaluate(application);   // ← was: RiskDecision.Review

    db.LoanApplications.Add(application);
    await db.SaveChangesAsync();

    // auto-embed the new application for RAG
    string content =
        $"Application {application.Id}: {application.FullName}, age {application.Age}. " +
        $"Loan of {application.CreditAmount:C0} over {application.DurationMonths} months for {FeatureLabels.Purpose(application.Purpose)}. " +
        $"Checking: {FeatureLabels.CheckingStatus(application.CheckingStatus)}. " +
        $"Credit history: {FeatureLabels.CreditHistory(application.CreditHistory)}. " +
        $"Employment: {FeatureLabels.Employment(application.Employment)}. " +
        $"Decision: {application.Decision}.";

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
    builder.AddOllamaChatCompletion("llama3.2", new Uri("http://localhost:11434"));
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
        string content =
            $"Application {app.Id}: {app.FullName}, age {app.Age}. " +
            $"Loan of {app.CreditAmount:C0} over {app.DurationMonths} months for {FeatureLabels.Purpose(app.Purpose)}. " +
            $"Checking: {FeatureLabels.CheckingStatus(app.CheckingStatus)}. " +
            $"Credit history: {FeatureLabels.CreditHistory(app.CreditHistory)}. " +
            $"Employment: {FeatureLabels.Employment(app.Employment)}. " +
            $"Decision: {app.Decision}.";

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

app.MapGet("/api/ask", async (string question, CreditRiskDbContext db, EmbeddingService embedder) =>
{
    // 1. embed the question
    var questionVector = await embedder.GetEmbeddingAsync(question);

    // 2. similarity search — find the 3 most relevant applications
    var relevant = await db.ApplicationEmbeddings
        .OrderBy(e => e.Embedding.CosineDistance(questionVector))
        .Take(3)
        .Select(e => e.Content)
        .ToListAsync();

    // 3. build the grounded prompt
    string context = string.Join("\n", relevant);
    string prompt =
        $"You are a credit risk assistant. Answer the question using ONLY the application data below.\n\n" +
        $"Application data:\n{context}\n\n" +
        $"Question: {question}\n\nAnswer:";

    // 4. ask the LLM
    var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
    builder.AddOllamaChatCompletion("llama3.2", new Uri("http://localhost:11434"));
    var kernel = builder.Build();
    var result = await kernel.InvokePromptAsync(prompt);

    return Results.Ok(new { question, answer = result.ToString(), sourcesUsed = relevant.Count });
});

app.Run();