using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Data.Common;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DbParameterExtensions
    {
        public static DbParameter Copy(this DbParameter p)
        {
            return new DbParameterBuilder()
                        .WithName(p.ParameterName)
                        .WithType(p.DbType)
                        .WithDirection(p.Direction)
                        .WithValue(p.Value)
                        .Build();
        }

        public static DbParameter[] Copy(this DbParameter[] parameters)
        {
            var len = parameters == null ? 0 : parameters.Length;
            var result = new DbParameter[len];

            for (int i = 0; i < len; i++)
            {
                result[i] = parameters[i].Copy();
            }

            return result;
        }

        public static int GetInt32(this DbParameter parameter)
        {
            var v = parameter.Value;
            return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }

        public static long GetInt64(this DbParameter parameter)
        {
            var v = parameter.Value;
            return v == null || v == DBNull.Value ? 0 : Convert.ToInt64(v);
        }
    }
}
