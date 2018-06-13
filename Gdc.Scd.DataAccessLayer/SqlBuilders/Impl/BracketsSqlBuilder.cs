using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class BracketsSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder SqlBuilder { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"{this.SqlBuilder.Build(context)}";
        }
    }
}
