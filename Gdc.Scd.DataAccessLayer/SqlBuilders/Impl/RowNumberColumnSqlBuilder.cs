using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class RowNumberColumnSqlBuilder : ISqlBuilder
    {
        public IEnumerable<OrderByInfo> OrderByInfos { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var orderBy = new OrderBySqlBuilder
            {
                Query = new RawSqlBuilder(string.Empty),
                OrderByInfos = this.OrderByInfos
            };

            return $" ROW_NUMBER() OVER ({orderBy.Build(context)})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
