using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class BaseSqlHelper
    {
        private readonly ISqlBuilder sqlBuilder;

        protected List<CommandParameterInfo> Parameters { get; set; } = new List<CommandParameterInfo>();

        public BaseSqlHelper(ISqlBuilder sqlBuilder)
        {
            this.sqlBuilder = sqlBuilder;
        }

        public string ToSql()
        {
            return this.sqlBuilder.Build(new SqlBuilderContext());
        }

        public ISqlBuilder ToSqlBuilder()
        {
            return this.sqlBuilder;
        }

        public IEnumerable<CommandParameterInfo> GetParameters()
        {
            return this.Parameters.AsReadOnly();
        }
    }
}
