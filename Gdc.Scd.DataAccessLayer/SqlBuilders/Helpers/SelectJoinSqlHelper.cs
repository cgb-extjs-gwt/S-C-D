using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectJoinSqlHelper : SqlHelper
    {
        public SelectJoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SelectJoinSqlHelper Join()
        {
            throw new NotImplementedException();
        }

        public SqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
