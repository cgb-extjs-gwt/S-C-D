using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class TableSqlBuilder : NameSqlBuilder
    {
        public string DataBase { get; set; }

        public string Schema { get; set; }

        public TableSqlBuilder()
        {
        }

        public TableSqlBuilder(string tableName, string schema = null, string database = null)
        {
            this.Name = tableName;
            this.Schema = schema;
            this.DataBase = database;
        }

        public TableSqlBuilder(BaseEntityMeta meta, string database = null)
            : this(meta.Name, meta.Schema, database)
        {
        }

        public override string Build(SqlBuilderContext context)
        {
            var stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(this.DataBase))
            {
                stringBuilder.Append(this.GetSqlName(this.DataBase));
                stringBuilder.Append(".");
            }

            if (!string.IsNullOrWhiteSpace(this.Schema))
            {
                stringBuilder.Append(this.GetSqlName(this.Schema));
                stringBuilder.Append(".");
            }

            stringBuilder.Append(this.GetSqlName(this.Name));

            return stringBuilder.ToString();
        }
    }
}
