using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters
{
    /// <summary>
    /// Create MS SQL parameter
    /// </summary>
    public class DbParameterBuilder
    {
        private SqlParameter p;

        public DbParameterBuilder()
        {
            p = new SqlParameter();
        }

        public DbParameter Build() { return p; }

        public DbParameterBuilder WithName(string name)
        {
            p.ParameterName = name;
            return this;
        }

        public DbParameterBuilder WithType(DbType type)
        {
            p.DbType = type;
            return this;
        }

        public DbParameterBuilder WithTypeName(string typeName)
        {
            p.TypeName = typeName;
            return this;
        }

        public DbParameterBuilder WithValue(object value)
        {
            return WithPValue(value);
        }

        public DbParameterBuilder WithValue(long? value)
        {
            return value.HasValue ? WithPValue(value.Value) : WithNull();
        }

        public DbParameterBuilder WithValue(long value)
        {
            return WithPValue(value);
        }

        public DbParameterBuilder WithValue(int? value)
        {
            return value.HasValue ? WithPValue(value.Value) : WithNull();
        }

        public DbParameterBuilder WithValue(int value)
        {
            return WithPValue(value);
        }

        public DbParameterBuilder WithValue(DataTable value)
        {
            return WithPValue(value);
        }

        public DbParameterBuilder WithListIdValue(long[] values)
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

        public DbParameterBuilder WithKeyValue(IDictionary<string, string> values)
        {
            var tbl = new DataTable();

            tbl.Columns.Add(new DataColumn("key", typeof(string)) { MaxLength = 100 });
            tbl.Columns.Add(new DataColumn("value", typeof(string)) { MaxLength = 4000 });

            if (values != null)
            {
                var rows = tbl.Rows;
                foreach (var v in values)
                {
                    if (v.Value == null)
                    {
                        rows.Add(v.Key);
                    }
                    else
                    {
                        rows.Add(v.Key, v.Value);
                    }
                }
            }

            return WithTypeName("KeyValuePair").WithPValue(tbl);
        }

        public DbParameterBuilder WithNull()
        {
            return WithPValue(DBNull.Value);
        }

        public DbParameterBuilder WithDirection(ParameterDirection pdir)
        {
            p.Direction = pdir;
            return this;
        }

        private DbParameterBuilder WithPValue(object v)
        {
            p.Value = v;
            return this;
        }

        public static DbParameter CreateOutputParam(string pname, DbType type)
        {
            return new DbParameterBuilder().WithName(pname).WithType(type).WithDirection(ParameterDirection.Output).Build();
        }

        public static DbParameter Create(string pname, int pvalue)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, int? pvalue)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, long pvalue)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname, long? pvalue)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(pvalue).Build();
        }

        public static DbParameter Create(string pname)
        {
            return new DbParameterBuilder().WithName(pname).WithNull().Build();
        }

        public static DbParameter Create(string pname, object value)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(value).Build();
        }

        public static DbParameter Create(DbType type, string pname, object value)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(value).WithType(type).Build();
        }

        public static DbParameter Create(string typeName, string pname, DataTable value)
        {
            return new DbParameterBuilder().WithName(pname).WithValue(value).WithTypeName(typeName).Build();
        }

        public static DbParameter CreateListID(string pname, long[] values)
        {
            return new DbParameterBuilder().WithName(pname).WithListIdValue(values).Build();
        }
    }
}
