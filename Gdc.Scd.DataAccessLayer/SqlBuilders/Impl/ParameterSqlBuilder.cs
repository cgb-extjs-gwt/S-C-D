using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ParameterSqlBuilder : ISqlBuilder
    {
        public CommandParameterInfo ParamInfo { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"@{this.ParamInfo.Name}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
