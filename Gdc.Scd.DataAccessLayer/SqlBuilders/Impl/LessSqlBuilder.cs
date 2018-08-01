namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class LessSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "<";
        }
    }
}
