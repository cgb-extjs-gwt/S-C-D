using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public abstract class BaseColumnInfo
    {
        public string Alias { get; set; }

        protected BaseColumnInfo(string alias)
        {
            this.Alias = alias;
        }

        protected BaseColumnInfo()
        {
        }
    }
}
