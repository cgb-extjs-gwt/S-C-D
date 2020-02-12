using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AddColumnsSqlBuilder : ISqlBuilder
    {
        public IEnumerable<ISqlBuilder> Columns { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var columns = 
                string.Join(
                    $", {Environment.NewLine}", 
                    this.Columns.Select(columnQuery => columnQuery.Build(context)));

            return $"ADD{Environment.NewLine}{columns}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Columns;
        }
    }
}
