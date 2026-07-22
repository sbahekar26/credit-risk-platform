namespace CreditRisk.Core;

public class LoanApplication
{
    public int Id { get; set; }

    // Applicant basics
    public required string FullName { get; set; }
    public int Age { get; set; }

    // The German-credit features we collect on the form
    public required string CheckingStatus { get; set; }   // A11-A14
    public int DurationMonths { get; set; }
    public required string CreditHistory { get; set; }     // A30-A34
    public required string Purpose { get; set; }           // A40-A410
    public decimal CreditAmount { get; set; }
    public required string Savings { get; set; }           // A61-A65
    public required string Employment { get; set; }        // A71-A75
    public required string Housing { get; set; }           // A151-A153

    // The decision
    public RiskDecision Decision { get; set; }
    public DateTime SubmittedOn { get; set; }
}