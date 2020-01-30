using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ParameterSqlBuilder : ISqlBuilder
    {
        public CommandParameterInfo ParamInfo { get; set; }

        public ParameterSqlBuilder()
        {
        }

        public ParameterSqlBuilder(object value)
        {
            this.ParamInfo = new CommandParameterInfo(value);
        }

        public string Build(SqlBuilderContext context)
        {
            return context.GetParameterName(this.ParamInfo.Value);
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
