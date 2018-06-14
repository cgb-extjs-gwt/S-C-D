using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectSqlHelper : BaseSqlHelper
    {
        public SelectSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public FromSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null)
        {
            return new FromSqlHelper(new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = new TableSqlBuilder
                {
                    DataBase = dataBaseName,
                    Schema = schemaName,
                    Name = tabeName
                }
            });
        }

        public FromSqlHelper FromQuery(ISqlBuilder query)
        {
            return new FromSqlHelper(new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = new BracketsSqlBuilder
                {
                    SqlBuilder = query
                }
            });
        }

        public FromSqlHelper FromQuery(BaseSqlHelper sqlHelper)
        {
            return this.FromQuery(sqlHelper.ToSqlBuilder());
        }
    }
}
