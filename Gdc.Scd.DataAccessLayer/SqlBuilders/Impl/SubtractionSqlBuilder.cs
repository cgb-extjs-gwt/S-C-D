namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class SubtractionSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "-";
        }
    }
}
