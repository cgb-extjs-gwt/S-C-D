using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public abstract class BinaryOperatorSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder LeftOperator { get; set; }

        public ISqlBuilder RightOperator { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"{this.LeftOperator.Build(context)} {this.GetOperator()} {this.RightOperator.Build(context)}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.LeftOperator;
            yield return this.RightOperator;
        }

        protected abstract string GetOperator();
    }
}
