using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ColumnSqlBuilder : NameSqlBuilder
    {
        public string Table { get; set; }

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
