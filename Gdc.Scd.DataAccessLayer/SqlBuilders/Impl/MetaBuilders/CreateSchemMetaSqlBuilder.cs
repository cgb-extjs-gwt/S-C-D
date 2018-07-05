using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreateSchemMetaSqlBuilder : ISqlBuilder
    {
        public string Schema { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"CREATE SCHEMA [{this.Schema}] AUTHORIZATION [dbo]";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
