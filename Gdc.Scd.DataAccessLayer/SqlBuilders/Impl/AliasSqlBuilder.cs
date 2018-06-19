using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AliasSqlBuilder : BaseSqlBuilder
    {
        public string Alias { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.SqlBuilder.Build(context)} AS {this.Alias}";
        }
    }
}
