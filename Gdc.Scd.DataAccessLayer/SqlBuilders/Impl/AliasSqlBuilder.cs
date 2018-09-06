using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AliasSqlBuilder : BaseQuerySqlBuilder
    {
        public string Alias { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.Query.Build(context)} AS [{this.Alias}]";
        }
    }
}
