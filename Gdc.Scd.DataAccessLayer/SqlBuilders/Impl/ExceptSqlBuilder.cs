using System;
using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ExceptSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Left { get; set; }

        public ISqlBuilder Right { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var left = this.Left.Build(context);
            var right = this.Right.Build(context);

            return $"{left}{Environment.NewLine}EXCEPT{Environment.NewLine}{right}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Left;
            yield return this.Right;
        }
    }
}
