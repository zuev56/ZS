namespace Zs.Bot.Data.Queries;

public sealed class Group : ICondition
{
    public ICondition Condition1 { get; set; } = null!;
    public ICondition Condition2 { get; set; } = null!;
    public LogicalOperator Operator { get; set; }

    public override string ToString()
        => $"({Condition1}) {Operator} ({Condition2})";
}