using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IFromSqlHelper<T>
    {
        T From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null);

        T From(BaseEntityMeta meta, string alias = null);

        T FromQuery(ISqlBuilder query);

        T FromQuery(SqlHelper sqlHelper);
    }
}
