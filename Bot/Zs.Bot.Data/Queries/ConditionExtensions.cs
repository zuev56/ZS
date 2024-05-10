namespace Zs.Bot.Data.Queries;

public static class ConditionExtensions
{
    public static Group And(this ICondition mainCondition, ICondition otherCondition)
    {
        return new Group
        {
            Condition1 = mainCondition,
            Condition2 = otherCondition,
            Operator = LogicalOperator.And
        };
    }

    public static Group Or(this ICondition mainCondition, ICondition otherCondition)
    {
        return new Group
        {
            Condition1 = mainCondition,
            Condition2 = otherCondition,
            Operator = LogicalOperator.Or
        };
    }
}