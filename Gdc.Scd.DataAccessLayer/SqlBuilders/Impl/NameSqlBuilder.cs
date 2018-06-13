using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public abstract class NameSqlBuilder : ISqlBuilder
    {
        public string Name { get; set; }

        public abstract string Build(SqlBuilderContext context);

        protected string GetSqlName(string name)
        {
            return $"[{name}]";
        }
    }
}
