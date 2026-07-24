using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

namespace CreditRisk.Core;

public class LabelOutput
{
    public bool Label { get; set; }
}

[CustomMappingFactoryAttribute("LabelMapping")]
public class LabelMappingFactory : CustomMappingFactory<CreditData, LabelOutput>
{
    public override Action<CreditData, LabelOutput> GetMapping()
    {
        return (input, output) => { output.Label = input.LabelRaw == 2; };
    }
}