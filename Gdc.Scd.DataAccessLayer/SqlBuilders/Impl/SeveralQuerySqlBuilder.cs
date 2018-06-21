using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class SeveralQuerySqlBuilder : ISqlBuilder
    {
        public IEnumerable<ISqlBuilder> Queries { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return string.Join($";{Environment.NewLine}", this.Queries.Select(query => query.Build(context)));
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Queries;
        }
    }
}
