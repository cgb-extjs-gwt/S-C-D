using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AndSqlBuilder : BinaryOperatorSqlBuilder
    {
        protected override string GetOperator()
        {
            return "AND";
        }
    }
}
