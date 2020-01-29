using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DropIndexSqlBuilder : ISqlBuilder
    {
        public string IndexName { get; set; }

        public string ShemaName { get; set; }

        public string TableName { get; set; }

        public DropIndexSqlBuilder()
        {
        }

        public DropIndexSqlBuilder(string indexName, string tableName, string shemaName = null)
        {
            this.IndexName = indexName;
            this.TableName = tableName;
            this.ShemaName = shemaName;
        }

        public DropIndexSqlBuilder(string indexName, BaseEntityMeta meta)
            : this(indexName, meta.Name, meta.Schema)
        {
        }

        public string Build(SqlBuilderContext context)
        {
            var table = new TableSqlBuilder(this.TableName, this.ShemaName).Build(context);

            return $"DROP INDEX {this.IndexName} ON {table}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
