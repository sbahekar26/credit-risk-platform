public class LoanApplication
{
    public int Id { get; set; }
    public required Applicant Applicant { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public required string Purpose { get; set; }
    public DateTime SubmittedOn { get; set; }
    public RiskDecision Decision { get; set; }
}