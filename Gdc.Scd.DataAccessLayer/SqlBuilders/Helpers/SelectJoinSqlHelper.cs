using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectJoinSqlHelper : BaseSqlHelper
    {
        public SelectJoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SelectJoinSqlHelper Join()
        {
            throw new NotImplementedException();
        }

        public BaseSqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
