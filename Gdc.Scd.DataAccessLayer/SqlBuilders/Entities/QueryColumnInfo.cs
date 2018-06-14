using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class QueryColumnInfo : BaseColumnInfo
    {
        public ISqlBuilder Query { get; set; }

        public QueryColumnInfo(ISqlBuilder query, string alias = null)
            : base(alias)
        {
            this.Query = query;
        }

        public QueryColumnInfo()
        {
        }
    }
}
