using Microsoft.ML;
using Microsoft.ML.Data;

MLContext mlContext = new MLContext(seed: 0);

// 1. Load the categorical data
IDataView data = mlContext.Data.LoadFromTextFile<CreditData>(
    path: "german.data",
    separatorChar: ' ',
    hasHeader: false);

// 2. Split 80/20
var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

// 3. Names of the categorical (string) columns to one-hot encode
string[] categoricalColumns = new[]
{
    "CheckingStatus", "CreditHistory", "Purpose", "Savings", "Employment",
    "PersonalStatusSex", "OtherDebtors", "Property", "OtherInstallmentPlans",
    "Housing", "Job", "Telephone", "ForeignWorker"
};

// 4. Names of the numeric columns
string[] numericColumns = new[]
{
    "Duration", "CreditAmount", "InstallmentRate", "ResidenceSince",
    "Age", "ExistingCredits", "LiablePeople"
};

// 5. Build the pipeline
var pipeline = mlContext.Transforms.CustomMapping<CreditData, LabelOutput>(
        (input, output) => { output.Label = input.LabelRaw == 2; },
        contractName: "LabelMapping")
    // one-hot encode every categorical column
    .Append(mlContext.Transforms.Categorical.OneHotEncoding(
        categoricalColumns.Select(c => new InputOutputColumnPair(c)).ToArray()))
    // combine everything into one Features vector
    .Append(mlContext.Transforms.Concatenate("Features",
        categoricalColumns.Concat(numericColumns).ToArray()))
    // the trainer
    .Append(mlContext.BinaryClassification.Trainers.FastTree(
        labelColumnName: "Label",
        featureColumnName: "Features"));

// 6. Train
Console.WriteLine("Training model...");
ITransformer model = pipeline.Fit(split.TrainSet);

// 7. Evaluate
var predictions = model.Transform(split.TestSet);
var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label");

Console.WriteLine($"Accuracy:  {metrics.Accuracy:P2}");
Console.WriteLine($"AUC:       {metrics.AreaUnderRocCurve:P2}");
Console.WriteLine($"F1 Score:  {metrics.F1Score:P2}");

// 8. Save
mlContext.Model.Save(model, data.Schema, "credit-model.zip");
Console.WriteLine("Model saved to credit-model.zip");

public class LabelOutput
{
    public bool Label { get; set; }
}