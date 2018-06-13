using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class WhereSqlHelper : BaseSqlHelper
    {
        public WhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public WhereSqlBuilder Where(ISqlBuilder condition)
        {
            return new WhereSqlBuilder
            {
                Condition = condition,
                SqlBuilder = this.ToSqlBuilder()
            };
        }
    }
}
