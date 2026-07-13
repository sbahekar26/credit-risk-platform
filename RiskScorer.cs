public class RiskScorer
{
    public string Evaluate (LoanApplication application)
    {
        //hard rule: age 18+, credit score 650+, loan amount < 50% of annual income
        //soft rule: purpose should be in line with person's background, credit score of 550-650 acceptable but needs review
        decimal ratio = application.LoanAmount / application.Applicant.AnnualIncome;
        if (application.Applicant.Age < 18)
        {
            return "Decline";
        }
        else if (application.Applicant.CreditScore < 550)
        {
            return "Decline";
        }
        else if (ratio > 0.5m)
        {
            return "Decline";
        }
        else if (application.Applicant.CreditScore < 650)
        {
            return "Review";
        }
        return "Approved";
    }
}