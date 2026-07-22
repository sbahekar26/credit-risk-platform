using CreditRisk.Core;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddDbContext<CreditRiskDbContext>(options => 
options.UseSqlite("Data Source = creditrisk.db"));
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/applications", async (LoanApplication application, CreditRiskDbContext db) =>
{
    application.SubmittedOn = DateTime.Now;
    application.Decision = RiskDecision.Review;   // placeholder until the model is wired in

    db.LoanApplications.Add(application);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        applicationId = application.Id,
        applicant = application.FullName,      // flat now, no .Applicant
        decision = application.Decision.ToString()
    });
});

app.MapGet("/api/applications", async (CreditRiskDbContext db) =>
    await db.LoanApplications.ToListAsync());

app.Run();