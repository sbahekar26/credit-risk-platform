using CreditRisk.Core;
using Xunit;

public class FeatureLabelsTests
{
    [Fact]
    public void CheckingStatus_KnownCode_ReturnsReadableText()
    {
        string result = FeatureLabels.CheckingStatus("A11");
        Assert.Equal("Negative balance (< 0 DM)", result);
    }

    [Fact]
    public void CheckingStatus_UnknownCode_ReturnsCodeUnchanged()
    {
        string result = FeatureLabels.CheckingStatus("ZZZ");
        Assert.Equal("ZZZ", result);
    }

    [Theory]
    [InlineData("A30", "No credits taken")]
    [InlineData("A34", "Critical account")]
    [InlineData("A32", "Existing credits paid duly")]
    public void CreditHistory_KnownCodes_ReturnReadableText(string code, string expected)
    {
        string result = FeatureLabels.CreditHistory(code);
        Assert.Equal(expected, result);
    }
}