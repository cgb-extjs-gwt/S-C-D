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
        private SqlParameter p;

        public SqlParameterBuilder()
        {
            p = new SqlParameter();
        }

        public DbParameter Build() { return p; }

        public SqlParameterBuilder WithName(string name)
        {
            p.ParameterName = name;
            return this;
        }

        public SqlParameterBuilder WithType(DbType type)
        {
            p.DbType = type;
            return this;
        }

        public SqlParameterBuilder WithTypeName(string typeName)
        {
            p.TypeName = typeName;
            return this;
        }

        public SqlParameterBuilder WithValue(object value)
        {
            return WithPValue(value);
        }

        public SqlParameterBuilder WithValue(long? value)
        {
            return value.HasValue ? WithPValue(value.Value) : WithNull();
        }

        public SqlParameterBuilder WithValue(long value)
        {
            return WithPValue(value);
        }

        public static DbParameter CreateOutputParam(string pname, SqlDbType type)
        {
            var param = new SqlParameter(pname, type)
            {
                Direction = ParameterDirection.Output
            };
            return param;
        }

        public SqlParameterBuilder WithValue(int? value)
        {
            return value.HasValue ? WithPValue(value.Value) : WithNull();
        }

        public SqlParameterBuilder WithValue(int value)
        {
            return WithPValue(value);
        }

        public SqlParameterBuilder WithValue(DataTable value)
        {
            return WithPValue(value);
        }

        public SqlParameterBuilder WithListIdValue(long[] values)
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

            return WithTypeName("ListID").WithPValue(tbl);
        }

        public SqlParameterBuilder WithNull()
        {
            return WithPValue(DBNull.Value);
        }

        private SqlParameterBuilder WithPValue(object v)
        {
            p.Value = v;
            return this;
        }

        public static DbParameter Create(string pname, int pvalue)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, int? pvalue)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, long pvalue)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, long? pvalue)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname)
        {
            return new SqlParameterBuilder().WithName(pname).WithNull().Build();
        }

        public static DbParameter Create(string pname, object value)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(value).Build();
        }

        public static DbParameter Create(DbType type, string pname, object value)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(value).WithType(type).Build();
        }

        public static DbParameter Create(string typeName, string pname, DataTable value)
        {
            return new SqlParameterBuilder().WithName(pname).WithValue(value).WithTypeName(typeName).Build();
        }

        public static DbParameter CreateListID(string pname, long[] values)
        {
            return new SqlParameterBuilder().WithName(pname).WithListIdValue(values).Build();
        }
    }
}
