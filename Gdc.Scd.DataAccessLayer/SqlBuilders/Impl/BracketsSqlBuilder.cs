using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class BracketsSqlBuilder : BaseSqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"({this.SqlBuilder.Build(context)})";
        }
    }
}
