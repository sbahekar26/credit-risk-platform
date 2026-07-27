namespace CreditRisk.Core;

public static class FeatureLabels
{
    public static string CheckingStatus(string code) => code switch
    {
        "A11" => "Negative balance (< 0 DM)",
        "A12" => "0 – 200 DM",
        "A13" => "200+ DM",
        "A14" => "No checking account",
        _ => code
    };

    public static string CreditHistory(string code) => code switch
    {
        "A30" => "No credits taken",
        "A31" => "All paid back duly",
        "A32" => "Existing credits paid duly",
        "A33" => "Past payment delays",
        "A34" => "Critical account",
        _ => code
    };

    public static string Purpose(string code) => code switch
    {
        "A40" => "Car (new)",
        "A41" => "Car (used)",
        "A42" => "Furniture/equipment",
        "A43" => "Radio/TV",
        "A46" => "Education",
        "A49" => "Business",
        _ => code
    };

    public static string Savings(string code) => code switch
    {
        "A61" => "< 100 DM",
        "A62" => "100 – 500 DM",
        "A63" => "500 – 1000 DM",
        "A64" => "1000+ DM",
        "A65" => "Unknown / none",
        _ => code
    };

    public static string Employment(string code) => code switch
    {
        "A71" => "Unemployed",
        "A72" => "< 1 year",
        "A73" => "1 – 4 years",
        "A74" => "4 – 7 years",
        "A75" => "7+ years",
        _ => code
    };

    public static string Housing(string code) => code switch
    {
        "A151" => "Rent",
        "A152" => "Own",
        "A153" => "For free",
        _ => code
    };
}