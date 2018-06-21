using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class TableSqlBuilder : NameSqlBuilder
    {
        public string DataBase { get; set; }

        public string Schema { get; set; }

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
