using CreditRisk.Core;
using Xunit;

public class DecisionThresholdsTests
{
    [Theory]
    [InlineData(0.9f, RiskDecision.Decline)]
    [InlineData(0.6f, RiskDecision.Decline)]     // boundary
    [InlineData(0.59f, RiskDecision.Review)]     // just below
    [InlineData(0.35f, RiskDecision.Review)]     // boundary
    [InlineData(0.34f, RiskDecision.Approved)]   // just below
    [InlineData(0.0f, RiskDecision.Approved)]
    public void FromProbability_MapsToCorrectDecision(float probability, RiskDecision expected)
    {
        RiskDecision result = DecisionThresholds.FromProbability(probability);
        Assert.Equal(expected, result);
    }
}