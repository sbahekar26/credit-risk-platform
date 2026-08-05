namespace CreditRisk.Core;

public static class DecisionThresholds
{
    public static RiskDecision FromProbability(float probability)
    {
        if (probability >= 0.6f) return RiskDecision.Decline;
        if (probability >= 0.35f) return RiskDecision.Review;
        return RiskDecision.Approved;
    }
}

