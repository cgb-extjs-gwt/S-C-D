using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class ValueUpdateColumnInfo : BaseUpdateColumnInfo
    {
        public object Value { get; set; }

        public string ParameterName { get; set; }

        public ValueUpdateColumnInfo()
        {
        }

        public ValueUpdateColumnInfo(string name, object value, string parameterName = null)
            : base(name)
        {
            this.Value = value;
            this.ParameterName = parameterName;
        }
    }
}
