using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);


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
    RiskScorer scorer = new RiskScorer();
    RiskDecision decision = scorer.Evaluate(application);
    application.Decision = decision;
    db.LoanApplications.Add(application);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        applicationId = application.Id,
        applicant = application.Applicant.FullName,
        decision = decision.ToString()
    });
});

app.MapGet("/api/applications", async (CreditRiskDbContext db) =>
    await db.LoanApplications.Include(a => a.Applicant).ToListAsync());

app.Run();