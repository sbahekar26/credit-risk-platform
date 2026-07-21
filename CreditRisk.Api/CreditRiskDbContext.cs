using Microsoft.EntityFrameworkCore;

public class CreditRiskDbContext : DbContext
{
    public CreditRiskDbContext(DbContextOptions<CreditRiskDbContext> options) : base(options)
    {
        
    }
    public DbSet<LoanApplication> LoanApplications { get; set; }
    public DbSet<Applicant> Applicants { get; set; }
}