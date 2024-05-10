namespace Zs.Bot.Data.Queries;

// Пока не используется
public sealed class Column : ICondition
{
    public string ColumnName { get; init; } = null!;
    public object Value { get; init; } = null!;
    public ComparisonOperator Operator { get; init; }

    public static Column Eq(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Eq };
    public static Column Ne(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Ne };
    public static Column Gt(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Gt };
    public static Column Gte(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Gte };
    public static Column Lt(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Lt };
    public static Column Lte(string columnName, object value) => new() { ColumnName = columnName, Value = value, Operator = ComparisonOperator.Lte };

    public override string ToString()
        => $"{ColumnName} {Operator} {Value}";
}