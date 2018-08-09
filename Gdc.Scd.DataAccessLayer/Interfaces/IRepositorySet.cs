using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        ITransaction GetTransaction();

        Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null);

        Task<IEnumerable<T>> ReadBySql<T>(SqlHelper query, Func<IDataReader, T> mapFunc);

        int ExecuteSql(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        int ExecuteSql(SqlHelper query);

        Task<int> ExecuteSqlAsync(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        Task<int> ExecuteSqlAsync(SqlHelper query);

        int ExecuteProc(string procName, params DbParameter[] parameters);

        Task<int> ExecuteProcAsync(string procName, params DbParameter[] parameters);

        Task<List<T>> ExecuteProcAsync<T>(string procName, params DbParameter[] parameters)
            where T : new();

        IEnumerable<Type> GetRegisteredEntities();
    }
}
