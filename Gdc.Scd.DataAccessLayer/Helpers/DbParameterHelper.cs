using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Data.Common;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DbParameterHelper
    {
        public static DbParameter Copy(this DbParameter p)
        {
            return new SqlParameterBuilder()
                        .WithName(p.ParameterName)
                        .WithType(p.DbType)
                        .WithDirection(p.Direction)
                        .WithValue(p.Value)
                        .Build();
        }

        public static int GetInt32(this DbParameter parameter)
        {
            var v = parameter.Value;
            return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }
    }
}
