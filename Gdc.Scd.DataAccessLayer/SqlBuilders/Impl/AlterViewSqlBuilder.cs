namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AlterViewSqlBuilder : BaseViewSqlBuilder
    {
        protected override string GetTypeSql() => "ALTER";
    }
}
