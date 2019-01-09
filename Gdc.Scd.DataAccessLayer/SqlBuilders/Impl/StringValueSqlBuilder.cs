using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class StringValueSqlBuilder : ISqlBuilder
    {
        public string Value { get; set; }

        public StringValueSqlBuilder()
        {
        }

        public StringValueSqlBuilder(string value)
        {
            this.Value = value;
        }

        public string Build(SqlBuilderContext context)
        {
            return $"'{this.Value}'";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
