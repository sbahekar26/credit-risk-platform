using CreditRisk.Core;
using Microsoft.ML;

public class CreditRiskPredictor
{
    private readonly PredictionEngine<CreditData, CreditPrediction> _engine;

    public CreditRiskPredictor()
    {
        MLContext mlContext = new MLContext();
        mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelMappingFactory).Assembly);

        ITransformer model = mlContext.Model.Load("credit-model.zip", out _);
        _engine = mlContext.Model.CreatePredictionEngine<CreditData, CreditPrediction>(model);
    }

    public RiskDecision Evaluate(LoanApplication app)
    {
        CreditData input = MapToModelInput(app);
        CreditPrediction prediction = _engine.Predict(input);

        return DecisionThresholds.FromProbability(prediction.Probability);
    }

    private CreditData MapToModelInput(LoanApplication app)
    {
        return new CreditData
        {
            CheckingStatus = app.CheckingStatus,
            Duration = app.DurationMonths,
            CreditHistory = app.CreditHistory,
            Purpose = app.Purpose,
            CreditAmount = (float)app.CreditAmount,
            Savings = app.Savings,
            Employment = app.Employment,
            Age = app.Age,
            Housing = app.Housing,

            InstallmentRate = 3,
            PersonalStatusSex = "A93",
            OtherDebtors = "A101",
            ResidenceSince = 2,
            Property = "A121",
            OtherInstallmentPlans = "A143",
            ExistingCredits = 1,
            Job = "A173",
            LiablePeople = 1,
            Telephone = "A191",
            ForeignWorker = "A201"
        };
    }
}