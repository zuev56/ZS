namespace Zs.Bot.Data.Queries;

public interface IQueryFactory
{
    RawDataStructure RawDataStructure { get; }
    string CreateFindByConditionQuery(string tableName, ICondition condition);
}