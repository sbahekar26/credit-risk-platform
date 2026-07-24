using CreditRisk.Core;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<CreditRiskPredictor>();
builder.Services.AddDbContext<CreditRiskDbContext>(options => 
options.UseSqlite("Data Source = creditrisk.db"));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/api/applications", async (LoanApplication application, CreditRiskDbContext db, CreditRiskPredictor predictor) =>
{
    application.SubmittedOn = DateTime.Now;
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

app.MapGet("/api/applications", async (CreditRiskDbContext db) =>
    await db.LoanApplications.ToListAsync());

app.Run();