namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class MultiplicationSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "*";
        }
    }
}
