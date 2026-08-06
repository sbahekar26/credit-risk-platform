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

    public DbSet<ApplicationEmbedding> ApplicationEmbeddings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    if (Database.IsNpgsql())
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.Entity<ApplicationEmbedding>()
            .Property(e => e.Embedding)
            .HasColumnType("vector(768)");
    }
    else
    {
        // in-memory/test provider can't map Vector — ignore the embeddings table
        modelBuilder.Ignore<ApplicationEmbedding>();
    }
}
}