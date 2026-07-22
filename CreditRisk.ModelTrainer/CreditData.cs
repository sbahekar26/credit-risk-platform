using Microsoft.ML.Data;

public class CreditData
{
    [LoadColumn(0)] public string CheckingStatus { get; set; } = "";
    [LoadColumn(1)] public float Duration { get; set; }
    [LoadColumn(2)] public string CreditHistory { get; set; } = "";
    [LoadColumn(3)] public string Purpose { get; set; } = "";
    [LoadColumn(4)] public float CreditAmount { get; set; }
    [LoadColumn(5)] public string Savings { get; set; } = "";
    [LoadColumn(6)] public string Employment { get; set; } = "";
    [LoadColumn(7)] public float InstallmentRate { get; set; }
    [LoadColumn(8)] public string PersonalStatusSex { get; set; } = "";
    [LoadColumn(9)] public string OtherDebtors { get; set; } = "";
    [LoadColumn(10)] public float ResidenceSince { get; set; }
    [LoadColumn(11)] public string Property { get; set; } = "";
    [LoadColumn(12)] public float Age { get; set; }
    [LoadColumn(13)] public string OtherInstallmentPlans { get; set; } = "";
    [LoadColumn(14)] public string Housing { get; set; } = "";
    [LoadColumn(15)] public float ExistingCredits { get; set; }
    [LoadColumn(16)] public string Job { get; set; } = "";
    [LoadColumn(17)] public float LiablePeople { get; set; }
    [LoadColumn(18)] public string Telephone { get; set; } = "";
    [LoadColumn(19)] public string ForeignWorker { get; set; } = "";
    [LoadColumn(20)] public float LabelRaw { get; set; }
}

public class CreditPrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedIsBad { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}