using CreditRisk.Core;
using Microsoft.EntityFrameworkCore;

public class ApplicationStatsService
{
    private readonly CreditRiskDbContext _db;

    public ApplicationStatsService(CreditRiskDbContext db)
    {
        _db = db;
    }

    public async Task<string?> TryAnswer(string question)
    {
        string q = question.ToLowerInvariant();

        // counting
        if (q.Contains("how many") || q.Contains("count") || q.Contains("number of"))
        {
            if (q.Contains("declin"))
                return $"{await Count(RiskDecision.Decline)} applications were declined.";
            if (q.Contains("approv"))
                return $"{await Count(RiskDecision.Approved)} applications were approved.";
            if (q.Contains("review"))
                return $"{await Count(RiskDecision.Review)} applications are in review.";
            return $"There are {await _db.LoanApplications.CountAsync()} applications in total.";
        }

        // highest / largest loan
        if ((q.Contains("highest") || q.Contains("largest") || q.Contains("biggest")) && q.Contains("loan")
            || (q.Contains("highest") || q.Contains("largest")) && q.Contains("amount"))
        {
            var top = await _db.LoanApplications
                .OrderByDescending(a => a.CreditAmount)
                .FirstOrDefaultAsync();
            return top is null ? "No applications found."
                : $"The highest loan amount is {top.CreditAmount:C0}, from {top.FullName} (Application {top.Id}).";
        }

        // lowest / smallest loan
        if ((q.Contains("lowest") || q.Contains("smallest")) && (q.Contains("loan") || q.Contains("amount")))
        {
            var bottom = await _db.LoanApplications
                .OrderBy(a => a.CreditAmount)
                .FirstOrDefaultAsync();
            return bottom is null ? "No applications found."
                : $"The lowest loan amount is {bottom.CreditAmount:C0}, from {bottom.FullName} (Application {bottom.Id}).";
        }

        // newest / most recent
        if (q.Contains("newest") || q.Contains("most recent") || q.Contains("latest"))
        {
            var newest = await _db.LoanApplications
                .OrderByDescending(a => a.SubmittedOn)
                .FirstOrDefaultAsync();
            return newest is null ? "No applications found."
                : $"The newest application is {newest.FullName} (Application {newest.Id}), submitted {newest.SubmittedOn:MMM d, yyyy}.";
        }

        // average loan
        if (q.Contains("average") && (q.Contains("loan") || q.Contains("amount")))
        {
            var avg = await _db.LoanApplications.AverageAsync(a => a.CreditAmount);
            return $"The average loan amount is {avg:C0}.";
        }

        // no structured match — signal caller to fall back to RAG
        return null;
    }

    private async Task<int> Count(RiskDecision decision) =>
        await _db.LoanApplications.CountAsync(a => a.Decision == decision);
}