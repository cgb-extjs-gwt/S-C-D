using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    public class CommonService
    {
        private readonly IRepositorySet _repo;

        public CommonService(IRepositorySet repo)
        {
            _repo = repo;
        }

        public DataTable ExecuteAsTable(string func, DbParameter[] parameters)
        {
            var sql = SelectQuery(func, parameters);

            return _repo.ExecuteAsTable(sql, parameters);
        }

        private string SelectQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        public DbParameter FillParameter(string name, string value)
        {
            var builder = new DbParameterBuilder();

            builder.WithName(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.WithValue(value);
            }
            else
            {
                builder.WithNull();
            }

            return builder.Build();
        }

        public static double CheckDoubleField(DataRow row, string fieldName)
        {
            if (row?.Field<double?>(fieldName) != null)
            {
                return row.Field<double>(fieldName);
            }
            return 0;
        }
    }
}
