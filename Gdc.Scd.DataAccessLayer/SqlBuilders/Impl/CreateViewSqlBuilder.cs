namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class CreateViewSqlBuilder : BaseViewSqlBuilder
    {
        protected override string GetTypeSql() => "CREATE";
    }
}
