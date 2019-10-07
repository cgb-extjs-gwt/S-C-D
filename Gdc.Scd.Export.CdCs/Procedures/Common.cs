using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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

        public List<T> ExecuteList<T>(string func, Func<DbDataReader, T> mapFunc, DbParameter[] parameters)
        {
            var sql = SelectQuery(func, parameters);

            return _repo.ExecuteAsList<T>(sql, mapFunc, parameters);
        }

        private string SelectQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Build();
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
