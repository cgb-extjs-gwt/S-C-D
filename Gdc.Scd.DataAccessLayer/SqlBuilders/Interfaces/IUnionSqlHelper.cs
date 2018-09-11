using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IUnionSqlHelper<TResult>
    {
        TResult Union(ISqlBuilder query, bool all = false);

        TResult Union(SqlHelper query, bool all = false);
    }
}
