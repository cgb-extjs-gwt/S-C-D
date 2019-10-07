using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IRepositorySet
    {
        IRepository<T> GetRepository<T>() where T : class, IIdentifiable, new();

        void Sync();

        ITransaction GetTransaction();

        Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null);

        Task<IEnumerable<T>> ReadBySql<T>(SqlHelper query, Func<IDataReader, T> mapFunc);

        Task ReadBySql(string sql, Action<DbDataReader> mapFunc, params DbParameter[] parameters);

        Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<DbDataReader, T> mapFunc, params DbParameter[] parameters);

        int ExecuteSql(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        int ExecuteSql(SqlHelper query);

        Task<int> ExecuteSqlAsync(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        Task<int> ExecuteSqlAsync(SqlHelper query);

        int ExecuteProc(string procName, params DbParameter[] parameters);

        Task<int> ExecuteProcAsync(string procName, params DbParameter[] parameters);

        int ExecuteProc(string procName, Action<DbDataReader> mapFunc, params DbParameter[] parameters);

        Task ExecuteProcAsync(string procName, Action<DbDataReader> mapFunc, params DbParameter[] parameters);

        List<T> ExecuteProc<T>(string procName, params DbParameter[] parameters) where T : new();

        Task<DataTable> ExecuteProcAsTableAsync(string procName, params DbParameter[] parameters);

        Task<(string json, int total)> ExecuteProcAsJsonAsync(string procName, params DbParameter[] parameters);

        Task<(string json, int total)> ExecuteAsJsonAsync(string sql, params DbParameter[] parameters);

        Task<Stream> ExecuteAsJsonStreamAsync(string sql, params DbParameter[] parameters);

        Task<DataTable> ExecuteAsTableAsync(string sql, params DbParameter[] parameters);

        DataTable ExecuteAsTable(string sql, params DbParameter[] parameters);

        List<T> ExecuteAsList<T>(string sql, Func<DbDataReader, T> mapFunc, params DbParameter[] parameters);

        Task<T> ExecuteScalarAsync<T>(string sql, params DbParameter[] parameters);

        Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<CommandParameterInfo> parameters = null);

        void Replace<T>(T oldEntity, T newEntity) where T : class;

        IEnumerable<Type> GetRegisteredEntities();
    }
}
