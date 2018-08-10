using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class OrderBySqlBuilder : BaseSqlBuilder
    {
        public IEnumerable<OrderByInfo> OrderByInfos { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var sql = this.SqlBuilder.Build(context);
            var orderBySqls = this.OrderByInfos.Select(info => $"{info.SqlBuilder.Build(context)} {info.Direction.ToString().ToUpper()}");

            return $"{sql} ORDER BY {string.Join(", ", orderBySqls)}";
        }
    }
}
