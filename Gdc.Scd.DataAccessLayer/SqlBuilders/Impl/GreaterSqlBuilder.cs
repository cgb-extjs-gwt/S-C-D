namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class GreaterSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return ">";
        }
    }
}
