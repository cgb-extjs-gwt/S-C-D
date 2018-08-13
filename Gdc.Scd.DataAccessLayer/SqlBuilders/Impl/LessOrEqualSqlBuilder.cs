namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class LessOrEqualSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "<=";
        }
    }
}
