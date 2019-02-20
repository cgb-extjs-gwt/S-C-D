namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DivisionSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "/";
        }
    }
}
