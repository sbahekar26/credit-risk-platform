using Microsoft.EntityFrameworkCore;
using CreditRisk.Core;                    // ← add

public class CreditRiskDbContext : DbContext
{
    public CreditRiskDbContext(DbContextOptions<CreditRiskDbContext> options)
        : base(options)
    {
    }

    public DbSet<LoanApplication> LoanApplications { get; set; }
    // delete the Applicants DbSet line — Applicant no longer exists
}