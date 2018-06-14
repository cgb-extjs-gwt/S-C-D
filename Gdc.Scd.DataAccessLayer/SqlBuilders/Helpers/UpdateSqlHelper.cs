using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UpdateSqlHelper : BaseSqlHelper
    {
        private readonly WhereSqlHelper whereHelper;

        public UpdateSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public BaseSqlHelper Where(ISqlBuilder condition)
        {
            return new BaseSqlHelper(this.whereHelper.Where(condition));
        }

        public BaseSqlHelper Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public BaseSqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new BaseSqlHelper(this.whereHelper.Where(filter, tableName));
        }
    }
}
