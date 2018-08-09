using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters
{
    /// <summary>
    /// Create MS SQL parameter
    /// </summary>
    public class SqlParameterBuilder
    {
        public static DbParameter Create(string pname, int pvalue)
        {
            return new SqlParameter(pname, pvalue);
        }

        public static DbParameter Create(string pname, int? pvalue)
        {
            if (pvalue.HasValue)
            {
                return Create(pname, pvalue.Value);
            }
            else
            {
                return Create(pname);
            }
        }

        public static DbParameter Create(string pname, long pvalue)
        {
            return new SqlParameter(pname, pvalue);
        }

        public static DbParameter Create(string pname, long? pvalue)
        {
            if (pvalue.HasValue)
            {
                return Create(pname, pvalue.Value);
            }
            else
            {
                return Create(pname);
            }
        }

        public static DbParameter Create(string pname)
        {
            return new SqlParameter(pname, DBNull.Value);
        }

        public static DbParameter Create(string pname, object value)
        {
            return new SqlParameter(pname, value);
        }

        public static DbParameter Create(DbType type, string pname, object value)
        {
            return new SqlParameter(pname, value) { DbType = type };
        }

        public static DbParameter Create(string typeName, string pname, DataTable value)
        {
            return new SqlParameter(pname, value) { TypeName = typeName };
        }

        public static DbParameter CreateListID(string pname, long[] values)
        {
            var tbl = new DataTable();
            tbl.Columns.Add("id", typeof(long));

            if (values != null)
            {
                var rows = tbl.Rows;
                for (var i = 0; i < values.Length; i++)
                {
                    rows.Add(values[i]);
                }
            }

            return Create("ListID", pname, tbl);
        }
    }
}
