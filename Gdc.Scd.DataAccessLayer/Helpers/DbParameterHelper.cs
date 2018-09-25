using System;
using System.Data.Common;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DbParameterHelper
    {
        public static int GetInt32(this DbParameter parameter)
        {
            var v = parameter.Value;
            return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }
    }
}
