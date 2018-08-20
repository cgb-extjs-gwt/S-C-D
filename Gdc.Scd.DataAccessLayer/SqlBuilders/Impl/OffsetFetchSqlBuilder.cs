using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class OffsetFetchSqlBuilder : BaseSqlBuilder
    {
        public int Offset { get; set; }

        public int? Fetch { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var fetchSql = this.Fetch.HasValue
                ? $" FETCH NEXT {this.Fetch} ROWS ONLY"
                : string.Empty;

            return $"{this.SqlBuilder.Build(context)} OFFSET {this.Offset} ROWS{fetchSql}";
        }
    }
}
