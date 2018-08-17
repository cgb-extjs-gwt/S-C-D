using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AverageSqlBuilder : BaseSqlBuilder
    {
        public bool IsDistinct { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var distinct = this.IsDistinct ? "DISTINCT " : string.Empty;
            var sql = this.SqlBuilder.Build(context);

            return $"AVG({distinct} {sql})";
        }
    }
}
