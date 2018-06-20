using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UpdateSqlHelper : SqlHelper
    {
        private readonly WhereSqlHelper whereHelper;

        public UpdateSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
        }

        public SqlHelper Where(ISqlBuilder condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter, tableName));
        }
    }
}
