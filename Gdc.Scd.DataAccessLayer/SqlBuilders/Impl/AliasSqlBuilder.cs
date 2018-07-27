using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AliasSqlBuilder : BaseSqlBuilder
    {
        public string Alias { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.SqlBuilder.Build(context)} AS [{this.Alias}]";
        }
    }
}
