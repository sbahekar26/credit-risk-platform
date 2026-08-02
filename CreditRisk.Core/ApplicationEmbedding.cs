using Pgvector;

namespace CreditRisk.Core;

public class ApplicationEmbedding
{
    public int Id { get; set; }
    public int LoanApplicationId { get; set; }
    public string Content { get; set; } = "";
    public Vector Embedding { get; set; } = new Vector(new float[768]);
}