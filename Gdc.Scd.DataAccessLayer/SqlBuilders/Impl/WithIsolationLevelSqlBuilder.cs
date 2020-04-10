using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using System;
using System.Data;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class WithIsolationLevelSqlBuilder : BaseQuerySqlBuilder
    {
        public IsolationLevel IsolationLevel { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            string query;

            switch (this.IsolationLevel)
            {
                case IsolationLevel.ReadUncommitted:
                case IsolationLevel.ReadCommitted:
                case IsolationLevel.RepeatableRead:
                case IsolationLevel.Serializable:
                case IsolationLevel.Snapshot:
                    query = $"{this.Query.Build(context)} WITH({this.IsolationLevel.ToString().ToUpper()})";
                    break;

                default:
                    throw new NotSupportedException();
            }

            return query;
        }
    }
}
