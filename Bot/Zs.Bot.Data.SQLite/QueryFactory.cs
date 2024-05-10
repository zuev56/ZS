using System;
using Zs.Bot.Data.Queries;

namespace Zs.Bot.Data.SQLite;

public sealed class QueryFactory : IQueryFactory
{
    public RawDataStructure RawDataStructure { get; }

    public QueryFactory(RawDataStructure rawDataStructure)
    {
        RawDataStructure = rawDataStructure;
    }

    public string CreateFindByConditionQuery(string tableName, ICondition condition)
    {
        var sqlConditionsPart = CreateSqlConditions(condition);

        sqlConditionsPart = sqlConditionsPart.StartsWith('(') && sqlConditionsPart.EndsWith(')')
            ? sqlConditionsPart[1..^1]
            : sqlConditionsPart;

        var fullQuery = $"select * from {tableName} where {sqlConditionsPart}";

        return fullQuery;
    }

    private string CreateSqlConditions(ICondition condition)
    {
        var sqlConditions = condition switch
        {
            Column columnCondition => CreateSqlForColumnCondition(columnCondition),
            RawData rawDataCondition => CreateSqlForRawDataCondition(rawDataCondition),
            Group conditionGroup => CreateSqlForConditionGroup(conditionGroup),
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
        };

        return sqlConditions;
    }

    private string CreateSqlForColumnCondition(Column condition)
    {
        // Пока решил не реализовывать
        throw new NotImplementedException();
    }

    private static string CreateSqlForRawDataCondition(RawData condition)
    {
        var sqlCondition = ConvertToSqlCondition(condition);

        return $"json_extract(RawData, '{condition.Path}') {sqlCondition}";
    }

    private string CreateSqlForConditionGroup(Group group)
    {
        var sqlFromCondition1 = CreateSqlConditions(group.Condition1);
        var sqlFromCondition2 = CreateSqlConditions(group.Condition2);
        var sqlPart = $"({sqlFromCondition1} {group.Operator} {sqlFromCondition2})";

        return sqlPart;
    }

    private static string ConvertToSqlCondition(RawData condition)
    {
        var value = condition switch
        {
            { Value:string } and { Operator: < ComparisonOperator.Contains } => $"'{condition.Value}'",
            _ => condition.Value
        };

        return condition.Operator switch
        {
            ComparisonOperator.Eq => $"= {value}",
            ComparisonOperator.Ne => $"!= {value}",
            ComparisonOperator.Gt => $"> {value}",
            ComparisonOperator.Gte => $">= {value}",
            ComparisonOperator.Lt => $"< {value}",
            ComparisonOperator.Lte => $"<= {value}",
            ComparisonOperator.Contains => $"like '%{value}%'",
            ComparisonOperator.DoesNotContain => $"not like '%{value}%'",
            ComparisonOperator.StartsWith => $"like '{value}%'",
            ComparisonOperator.DoesNotStartWith => $"not like '{value}%'",
            ComparisonOperator.EndsWith => $"like '%{value}'",
            ComparisonOperator.DoesNotEndWith => $"not like '%{value}'",
            _ => throw new ArgumentOutOfRangeException(nameof(condition.Operator))
        };
    }
}