using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public abstract class BaseUpdateColumnInfo
    {
        public string Name { get; set; }

        public BaseUpdateColumnInfo()
        {
        }

        public BaseUpdateColumnInfo(string name)
        {
            this.Name = name;
        }
    }
}
