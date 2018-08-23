using System;
using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class UnionSqlBuilder : ISqlBuilder
    {
        public bool All { get; set; }

        public ISqlBuilder Query1 { get; set; }

        public ISqlBuilder Query2 { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var allOption = this.All ? " ALL" : string.Empty;
            var leftQuery = $"{this.Query1.Build(context)}{Environment.NewLine}";
            var rightQuery = $" {Environment.NewLine}{this.Query2.Build(context)}";

            return $"{leftQuery}UNION{allOption}{rightQuery}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Query1;
            yield return this.Query2;
        }
    }
}
