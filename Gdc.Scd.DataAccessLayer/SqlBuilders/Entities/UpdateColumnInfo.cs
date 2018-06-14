using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class UpdateColumnInfo
    {
        public string Name { get; set; }

        public ISqlBuilder Query { get; set; }
    }
}
