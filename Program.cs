using System.Linq;

Applicant applicant = new Applicant
{
    FullName = "Sid Bahekar",
    Age = 18,
    AnnualIncome = 85000m,
    CreditScore = 600
};

List <LoanApplication> applications = new List<LoanApplication>
{
    new LoanApplication
    {
    Id = 1,
    Applicant = new Applicant { FullName = "Sid Bahekar", Age = 27, AnnualIncome = 85000m, CreditScore = 750 },
    LoanAmount = 25000m,
    LoanTermMonths = 36,
    Purpose = "Car Purchase",
    SubmittedOn = DateTime.Now
    },
    new LoanApplication
    {
        Id = 2,
        Applicant = new Applicant { FullName = "Rakesh Kumar", Age = 34, AnnualIncome = 45000m, CreditScore = 600 },
        LoanAmount = 15000m,
        LoanTermMonths = 24,
        Purpose = "Home Renovation",
        SubmittedOn = DateTime.Now
    },
    new LoanApplication
    {
        Id = 3,
        Applicant = new Applicant { FullName = "Amit Patel", Age = 22, AnnualIncome = 30000m, CreditScore = 520 },
        LoanAmount = 20000m,
        LoanTermMonths = 48,
        Purpose = "Debt Consolidation",
        SubmittedOn = DateTime.Now
    }
};

// LoanApplication application = applications[0];
// Console.WriteLine($"Application #{application.Id}");
// Console.WriteLine($"Application {application.Applicant.FullName}");
// Console.WriteLine($"Requesting: ${application.LoanAmount} over {application.LoanTermMonths}months"); 

RiskScorer scorer = new RiskScorer();

foreach (LoanApplication app in applications)
{
    RiskDecision decision = scorer.Evaluate(app);
    Console.WriteLine($"#{app.Id} {app.Applicant.FullName}: {decision}");
}

int approvedCount = applications.Count(app => scorer.Evaluate(app) == RiskDecision.Approved);
int declinedCount = applications.Count(app => scorer.Evaluate(app) == RiskDecision.Decline);
int reviewCount = applications.Count(app => scorer.Evaluate(app) == RiskDecision.Review);

Console.WriteLine($"\nSummary: {approvedCount} approved, {reviewCount} in review, {declinedCount} declined");