using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class QueryUpdateColumnInfo : BaseUpdateColumnInfo
    {
        public ISqlBuilder Query { get; set; }

        public QueryUpdateColumnInfo()
        {
        }

        public QueryUpdateColumnInfo(string name, ISqlBuilder query)
            : base(name)
        {
            this.Query = query;
        }
    }
}
