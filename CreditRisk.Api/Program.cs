using CreditRisk.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<CreditRiskPredictor>();
builder.Services.AddDbContext<CreditRiskDbContext>(options => 
options.UseNpgsql("Host=localhost;Port=5432;Database=creditrisk;Username=postgres;Password=devpassword"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();
app.UseCors();

app.UseHttpsRedirection();

app.MapPost("/api/applications", async (LoanApplication application, CreditRiskDbContext db, CreditRiskPredictor predictor) =>
{
    application.SubmittedOn = DateTime.UtcNow;
    application.Decision = predictor.Evaluate(application);   // ← was: RiskDecision.Review

    db.LoanApplications.Add(application);
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

app.Run();