using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DropViewSqlBuilder : TableSqlBuilder
    {
        public bool IfExists { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var ifExists = this.IfExists ? "IF EXISTS" : string.Empty;

            return $"DROP VIEW {ifExists} {base.Build(context)}";
        }
    }
}
