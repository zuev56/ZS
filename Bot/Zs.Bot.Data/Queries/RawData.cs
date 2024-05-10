namespace Zs.Bot.Data.Queries;

public class RawData : ICondition
{
    private RawData(string path, object value, ComparisonOperator @operator)
    {
        Path = path;
        Value = value;
        Operator = @operator;
    }

    public string Path { get; }
    public object Value { get; }
    public ComparisonOperator Operator { get; init; }

    public void Deconstruct(out string path, out object value, out ComparisonOperator @operator)
    {
        path = Path;
        value = Value;
        @operator = Operator;
    }

    public static RawData Eq(string path, object value) => new(path, value, ComparisonOperator.Eq);
    public static RawData Ne(string path, object value) => new(path, value, ComparisonOperator.Ne);
    public static RawData Gt(string path, object value) => new(path, value, ComparisonOperator.Gt);
    public static RawData Gte(string path, object value) => new(path, value, ComparisonOperator.Gte);
    public static RawData Lt(string path, object value) => new(path, value, ComparisonOperator.Lt);
    public static RawData Lte(string path, object value) => new(path, value, ComparisonOperator.Lte);
    public static RawData Contains(string path, object value) => new(path, value, ComparisonOperator.Contains);
    public static RawData StartsWith(string path, object value) => new(path, value, ComparisonOperator.StartsWith);
    public static RawData EndsWith(string path, object value) => new(path, value, ComparisonOperator.EndsWith);
    public static RawData DoesNotContain(string path, object value) => new(path, value, ComparisonOperator.DoesNotContain);
    public static RawData DoesNotStartWith(string path, object value) => new(path, value, ComparisonOperator.DoesNotStartWith);
    public static RawData DoesNotEndWith(string path, object value) => new(path, value, ComparisonOperator.DoesNotEndWith);

    public override string ToString()
        => $"{Path} {Operator} {Value}";
}