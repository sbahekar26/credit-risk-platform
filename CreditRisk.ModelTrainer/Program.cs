using Microsoft.ML;
using Microsoft.ML.Data;

// 1. Create the ML context — the entry point for everything ML.NET
MLContext mlContext = new MLContext(seed: 0);

// 2. Load the data from the file
IDataView data = mlContext.Data.LoadFromTextFile<CreditData>(
    path: "german-clean.data",
    separatorChar: ' ',
    hasHeader: false);

// 3. Split into training (80%) and test (20%) sets
DataOperationsCatalog.TrainTestData split =
    mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

// 4. Build the training pipeline
var pipeline = mlContext.Transforms.CustomMapping<CreditData, LabelOutput>(
        (input, output) => { output.Label = input.LabelRaw == 2; },
        contractName: "LabelMapping")
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: "Label",
        featureColumnName: "Features"));

// 5. Train the model
Console.WriteLine("Training model...");
ITransformer model = pipeline.Fit(split.TrainSet);

// 6. Evaluate on the test set
IDataView predictions = model.Transform(split.TestSet);
var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label");

Console.WriteLine($"Accuracy:  {metrics.Accuracy:P2}");
Console.WriteLine($"AUC:       {metrics.AreaUnderRocCurve:P2}");
Console.WriteLine($"F1 Score:  {metrics.F1Score:P2}");

// 7. Save the trained model to a file
mlContext.Model.Save(model, data.Schema, "credit-model.zip");
Console.WriteLine("Model saved to credit-model.zip");

// Helper class for the label mapping
public class LabelOutput
{
    public bool Label { get; set; }
}