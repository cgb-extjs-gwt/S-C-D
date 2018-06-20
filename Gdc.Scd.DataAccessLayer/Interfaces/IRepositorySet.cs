using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IRepositorySet
    {
        IRepository<T> GetRepository<T>() where T : class, IIdentifiable, new();

        void Sync();

        ITransaction BeginTransaction();

        Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null);

        Task<IEnumerable<T>> ReadBySql<T>(SqlHelper query, Func<IDataReader, T> mapFunc);

        Task<int> ExecuteSql(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        Task<int> ExecuteSql(SqlHelper query);
    }
}
