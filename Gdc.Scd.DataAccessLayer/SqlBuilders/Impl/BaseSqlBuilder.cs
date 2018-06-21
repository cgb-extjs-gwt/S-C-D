using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public abstract class BaseSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder SqlBuilder { get; set; }

        public abstract string Build(SqlBuilderContext context);

        public virtual IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.SqlBuilder;
        }
    }
}
