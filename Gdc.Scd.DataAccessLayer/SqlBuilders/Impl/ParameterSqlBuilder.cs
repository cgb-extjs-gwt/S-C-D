using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ParameterSqlBuilder : ISqlBuilder
    {
        public string Name { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"@{this.Name}";
        }
    }
}
