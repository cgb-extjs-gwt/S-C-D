using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class SetIdentityInsertSqlBuilder : ISqlBuilder
    {
        private readonly TableSqlBuilder tableSqlBuilder = new TableSqlBuilder();

        public string Schema 
        {
            get => this.tableSqlBuilder.Schema;
            set => this.tableSqlBuilder.Schema = value;
        }

        public string Table
        {
            get => this.tableSqlBuilder.Name;
            set => this.tableSqlBuilder.Name = value;
        }

        public bool On { get; set; }

        public SetIdentityInsertSqlBuilder()
        { 
        }

        public SetIdentityInsertSqlBuilder(string schema, string table, bool on)
        {
            this.Schema = schema;
            this.Table = table;
            this.On = on;
        }

        public SetIdentityInsertSqlBuilder(BaseEntityMeta meta, bool on)
            : this(meta.Schema, meta.Name, on)
        {
        }

        public string Build(SqlBuilderContext context)
        {
            var table = tableSqlBuilder.Build(context);
            var state = this.On ? "ON" : "OFF";

            return $"SET IDENTITY_INSERT {table} {state}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
