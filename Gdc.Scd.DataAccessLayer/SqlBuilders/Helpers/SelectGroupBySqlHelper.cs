using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectGroupBySqlHelper : BaseSqlHelper
    {
        public SelectGroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public BaseSqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
