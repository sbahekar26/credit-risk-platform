var builder = WebApplication.CreateBuilder(args);

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

app.MapPost("/api/applications", (LoanApplication application) =>
{
    RiskScorer scorer = new RiskScorer();
    RiskDecision decision = scorer.Evaluate(application);

    return Results.Ok(new
    {
        applicationId = application.Id,
        applicant = application.Applicant.FullName,
        decision = decision.ToString()
    });
});

app.Run();