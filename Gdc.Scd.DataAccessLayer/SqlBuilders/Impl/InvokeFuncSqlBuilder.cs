using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InvokeFuncSqlBuilder : ISqlBuilder
    {
        public string Func { get; set; }

        public string Shema { get; set; }

        public IEnumerable<ISqlBuilder> Params { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var parameters = this.Params == null 
                ? string.Empty
                : string.Join(", ", this.Params.Select(x => x.Build(context)));

            return $"[{this.Shema}].[{this.Func}]({parameters})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Params;
        }
    }
}
