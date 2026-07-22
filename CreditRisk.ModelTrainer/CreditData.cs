using Microsoft.ML.Data;

public class CreditData
{
    [LoadColumn(0, 23)]
    [VectorType(24)]
    public float[] Features { get; set; } = new float[24];

    [LoadColumn(24)]
    public float LabelRaw { get; set; }
}

public class CreditPrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedIsBad { get; set; }

    public float Probability { get; set; }
    public float Score { get; set; }
}