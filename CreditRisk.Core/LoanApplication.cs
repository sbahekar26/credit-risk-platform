using System.ComponentModel.DataAnnotations;

namespace CreditRisk.Core;

public class LoanApplication
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters")]
    public required string FullName { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Required]
    public required string CheckingStatus { get; set; }

    [Range(1, 120, ErrorMessage = "Duration must be 1–120 months")]
    public int DurationMonths { get; set; }

    [Required]
    public required string CreditHistory { get; set; }

    [Required]
    public required string Purpose { get; set; }

    [Range(1, 1000000, ErrorMessage = "Loan amount must be positive")]
    public decimal CreditAmount { get; set; }

    [Required]
    public required string Savings { get; set; }

    [Required]
    public required string Employment { get; set; }

    [Required]
    public required string Housing { get; set; }

    public RiskDecision Decision { get; set; }
    public DateTime SubmittedOn { get; set; }
}