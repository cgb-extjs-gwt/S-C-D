using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ColumnSqlBuilder : NameSqlBuilder
    {
        public string Table { get; set; }

        public ColumnSqlBuilder()
        {
        }

        public ColumnSqlBuilder(ColumnInfo columnInfo)
        {
            this.Name = columnInfo.Name;
            this.Table = columnInfo.TableName;
        }

        public override string Build(SqlBuilderContext context)
        {
            var stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(this.Table))
            {
                stringBuilder.Append(this.GetSqlName(this.Table));
                stringBuilder.Append(".");
            }

            stringBuilder.Append(this.GetSqlName(this.Name));

            return stringBuilder.ToString();
        }
    }
}
