using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public abstract class NameSqlBuilder : ISqlBuilder
    {
        public string Name { get; set; }

        public abstract string Build(SqlBuilderContext context);

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        protected string GetSqlName(string name)
        {
            return $"[{name}]";
        }
    }
}
