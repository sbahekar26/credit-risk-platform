Applicant applicant = new Applicant
{
    FullName = "Sid Bahekar",
    Age = 27,
    AnnualIncome = 85000m,
    CreditScore = 750
};

LoanApplication application = new LoanApplication
{
    Id = 1,
    Applicant = applicant,
    LoanAmount = 25000m,
    LoanTermMonths = 36,
    Purpose = "Car Purchase",
    SubmittedOn = DateTime.Now
};

Console.WriteLine($"Application #{application.Id}");
Console.WriteLine($"Application {application.Applicant.FullName}");
Console.WriteLine($"Requesting: ${application.LoanAmount} over {application.LoanTermMonths}months"); 