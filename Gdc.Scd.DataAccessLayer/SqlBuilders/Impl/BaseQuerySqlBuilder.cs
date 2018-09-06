using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public abstract class BaseQuerySqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public abstract string Build(SqlBuilderContext context);

        public virtual IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Query;
        }
    }
}
