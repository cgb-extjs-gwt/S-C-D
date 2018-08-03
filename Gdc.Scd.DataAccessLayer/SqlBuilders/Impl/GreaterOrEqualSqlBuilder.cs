namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class GreaterOrEqualSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return ">=";
        }
    }
}
