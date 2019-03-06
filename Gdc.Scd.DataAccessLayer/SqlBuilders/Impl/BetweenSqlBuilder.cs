using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class BetweenSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Column { get; set; }

        public ISqlBuilder Begin { get; set; }

        public ISqlBuilder End { get; set; }

        public bool IsNot { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var column = this.Column.Build(context);
            var begin = this.Begin.Build(context);
            var end = this.End.Build(context);
            var not = this.IsNot ? "NOT" : string.Empty;

            return $"{column} BETWEEN {not} {begin} AND {end}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Begin;
            yield return this.End;
        }
    }
}
