using System;
using System.Collections.Generic;
using System.Text;
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

            return $"{this.Query1.Build(context)}{Environment.NewLine}UNION{allOption}{Environment.NewLine}{this.Query2.Build(context)}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Query1;
            yield return this.Query2;
        }
    }
}
