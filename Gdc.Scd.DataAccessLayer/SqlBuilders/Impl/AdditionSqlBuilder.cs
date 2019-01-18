namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AdditionSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "+";
        }
    }
}
