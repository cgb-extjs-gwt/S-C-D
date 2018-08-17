using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class SelectSqlBuilder : ISqlBuilder
    {
        public bool IsDistinct { get; set; }

        public IEnumerable<ISqlBuilder> Columns { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var distinct = this.IsDistinct ? "DISTINCT" : string.Empty;

            var columnArray 

            var columns = string.Join(", ", this.Columns.Select(builder => builder.Build(context)));

            return $"SELECT {distinct} {columns}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Columns;
        }
    }
}
