using System.Data;
using System.Data.Common;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DbCommandExtensions
    {
        public static void AddParameters(this DbCommand cmd, params DbParameter[] parameters)
        {
            if(parameters == null)
            {
                return;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.Add(parameters[i]);
            }
        }

        public static void AddParameter(this DbCommand cmd, DbParameter parameter)
        {
            cmd.Parameters.Add(parameter);
        }

        public static void AsStoredProcedure(this DbCommand cmd, string procName, params DbParameter[] parameters)
        {
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.AddParameters(parameters);
        }
    }
}
