using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class CommandParameterInfo
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public DbType? Type { get; set; }
    }
}
